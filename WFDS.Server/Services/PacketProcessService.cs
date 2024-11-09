using WFDS.Godot.Binary;
using WFDS.Server.Common;
using WFDS.Server.Common.Helpers;
using WFDS.Server.Managers;
using Steamworks;

namespace WFDS.Server.Services;

public class PacketProcessService(ILogger<PacketProcessService> logger, PacketHandler packetHandler) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PacketProcessService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            TryProcessChannel(NetChannel.ActorUpdate);
            TryProcessChannel(NetChannel.ActorAction);
            TryProcessChannel(NetChannel.GameState);
            
            // TryProcessChannel(NetChannel.ActorAnimation);

#if false
            TryProcessChannel(NetChannel.Chalk);
            TryProcessChannel(NetChannel.Guitar);
            TryProcessChannel(NetChannel.Speech);
#endif
            await Task.Yield();
        }

        logger.LogInformation("PacketProcessService is stopping.");
    }

    private void TryProcessChannel(NetChannel channel)
    {
        try
        {
            ProcessChannel(channel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "failed to process channel {Channel}", channel);
        }
    }

    private void ProcessChannel(NetChannel channel)
    {
        while (SteamNetworking.IsP2PPacketAvailable(channel.Value))
        {
            var packet = SteamNetworking.ReadP2PPacket(channel.Value);
            if (!packet.HasValue)
            {
                break;
            }

            var data = packet.Value.Data;
            if (data.Length == 0)
            {
                continue;
            }

            try
            {
                var decompressed = GZipHelper.Decompress(data);
                var deserialized = GodotBinaryConverter.Deserialize(decompressed);
                packetHandler.OnPacketReceived(packet.Value.SteamId, channel, deserialized);
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "failed to packet processing");
                break;
            }
        }
    }
}