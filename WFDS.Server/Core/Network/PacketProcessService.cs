using System.Buffers;
using Steamworks;
using WFDS.Common.Helpers;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Godot.Binary;
using WFDS.Server.Core.Steam;

namespace WFDS.Server.Core.Network;

internal class PacketProcessService(ILogger<PacketProcessService> logger, PacketHandleManager packetHandleManager, ISessionManager sessionManager, SteamManager steam) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PacketProcessService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            TryProcessChannel(NetChannel.ActorUpdate);
            TryProcessChannel(NetChannel.ActorAction);
            TryProcessChannel(NetChannel.GameState);

            await Task.Delay(10, stoppingToken);
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

    private bool ValidatePacket(NetChannel channel, in CSteamID steamId, uint msgSize, uint readSize, byte[] bytes)
    {
        if (msgSize != readSize)
        {
            logger.LogError("failed to read packet from {Channel} (size mismatch {MsgSize}/{ReadSize})", channel, msgSize, readSize);
            return false;
        }
        
        if (sessionManager.IsBannedPlayer(steamId))
        {
            var packetDataBase64 = Convert.ToBase64String(bytes);
            logger.LogError("banned player {SteamId} tried to send packet: {PacketData}", steamId, packetDataBase64);
            return false;
        }

        if (bytes.Length != 0)
        {
            return true;
        }
        
        logger.LogError("empty packet from {SteamId}", steamId);
        return false;

    }

    private void ProcessChannel(NetChannel channel)
    {
        if (!steam.Initialized)
        {
            return;
        }
        
        while (SteamNetworking.IsP2PPacketAvailable(out var size, channel.Value))
        {
            var bytes = ArrayPool<byte>.Shared.Rent((int)size);
            var success = SteamNetworking.ReadP2PPacket(bytes, size, out var readSize, out var steamId, channel.Value);
            if (!success)
            {
                logger.LogError("failed to read packet from {Channel}", channel);
                continue;
            }
            
            if (!ValidatePacket(channel, steamId, size, readSize, bytes))
            {
                continue;
            }

            try
            {
                var decompressed = GZipHelper.Decompress(bytes, (int)readSize);
                var deserialized = GodotBinaryConverter.Deserialize(decompressed);
                packetHandleManager.OnPacketReceived(steamId, channel, deserialized);
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