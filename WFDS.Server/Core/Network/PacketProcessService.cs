using System.Buffers;
using Steamworks;
using WFDS.Common.Helpers;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using WFDS.Godot.Binary;
using ZLogger;

namespace WFDS.Server.Core.Network;

internal class PacketProcessService(ILogger<PacketProcessService> logger, PacketHandleManager packetHandleManager, SessionManager sessionManager, SteamManager steam) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.ZLogInformation($"PacketProcessService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            TryProcessChannel(NetChannel.ActorUpdate);
            TryProcessChannel(NetChannel.ActorAction);
            TryProcessChannel(NetChannel.GameState);
            TryProcessChannel(NetChannel.Chalk);

            await Task.Delay(10, stoppingToken);
        }

        logger.ZLogInformation($"PacketProcessService is stopping.");
    }

    private void TryProcessChannel(NetChannel channel)
    {
        try
        {
            ProcessChannel(channel);
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"failed to process channel {channel}");
        }
    }

    private bool ValidatePacket(NetChannel channel, in CSteamID steamId, uint msgSize, uint readSize, byte[] bytes)
    {
        if (msgSize != readSize)
        {
            logger.ZLogError($"failed to read packet from {channel} (size mismatch {msgSize}/{readSize})");
            return false;
        }

        if (sessionManager.IsBannedPlayer(steamId))
        {
            var packetDataBase64 = Convert.ToBase64String(bytes);
            logger.ZLogError($"banned player {steamId} tried to send packet: {packetDataBase64}");
            return false;
        }

        if (bytes.Length != 0)
        {
            return true;
        }

        logger.ZLogError($"empty packet from {steamId}");
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
            try
            {
                var success = SteamNetworking.ReadP2PPacket(bytes, size, out var readSize, out var steamId, channel.Value);
                if (!success)
                {
                    logger.ZLogError($"failed to read packet from {channel}");
                    continue;
                }

                if (!ValidatePacket(channel, steamId, size, readSize, bytes))
                {
                    continue;
                }

                using var stream = new MemoryStream(bytes, 0, (int)readSize);
                var decompressed = GZipHelper.Decompress(stream);
                var deserialized = GodotBinaryConverter.Deserialize(decompressed);
                packetHandleManager.OnPacketReceived(steamId, channel, deserialized);

            }
            catch (Exception ex)
            {
                logger.ZLogError(ex, $"failed to packet processing");
                break;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }
        }
    }
}