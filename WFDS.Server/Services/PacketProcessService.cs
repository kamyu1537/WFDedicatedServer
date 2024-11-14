using Steamworks;
using WFDS.Common.Helpers;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Godot.Binary;

namespace WFDS.Server.Services;

public class PacketProcessService(ILogger<PacketProcessService> logger, IPacketHandleManager packetHandleManager) : BackgroundService
{
    private static readonly NetChannel[] Channels =
    {
        NetChannel.ActorUpdate,
        NetChannel.ActorAction,
        NetChannel.GameState
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PacketProcessService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            TryProcessChannel(NetChannel.ActorUpdate);
            TryProcessChannel(NetChannel.ActorAction);
            TryProcessChannel(NetChannel.GameState);
#if false
            TryProcessChannel(NetChannel.ActorAnimation);
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
                packetHandleManager.OnPacketReceived(packet.Value.SteamId, channel, deserialized);
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