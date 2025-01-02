using System.Buffers;
using Steamworks;
using WFDS.Common.Helpers;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using WFDS.Godot.Binary;


namespace WFDS.Server.Core.Network;

internal class PacketProcessService(ILogger<PacketProcessService> logger, PacketHandleManager packetHandleManager, SessionManager sessionManager, SteamManager steam) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PacketProcessService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            TryProcessChannel(NetChannel.ActorUpdate, false);
            TryProcessChannel(NetChannel.ActorAction, false);
            TryProcessChannel(NetChannel.GameState, true);
            TryProcessChannel(NetChannel.Chalk, false);

            await Task.Delay(10, stoppingToken);
        }

        logger.LogInformation("PacketProcessService is stopping.");
    }

    private void TryProcessChannel(NetChannel channel, bool log)
    {
        try
        {
            ProcessChannel(channel, log);
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
            logger.LogError("failed to read packet from {Channel} (size mismatch {MessageSize}/{ReadSize})", channel, msgSize, readSize);
            return false;
        }

        if (sessionManager.IsBannedPlayer(steamId))
        {
            var packetDataBase64 = Convert.ToBase64String(bytes);
            logger.LogError("banned player {SteamId} tried to send packet: {PacketBytes}", steamId, packetDataBase64);
            return false;
        }

        if (bytes.Length != 0)
        {
            return true;
        }

        logger.LogError("empty packet from {SteamId}", steamId);
        return false;

    }

    private void ProcessChannel(NetChannel channel, bool log)
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
                    logger.LogError("failed to read packet from {Channel}", channel);
                    continue;
                }

                if (!ValidatePacket(channel, steamId, size, readSize, bytes))
                {
                    continue;
                }

                using var stream = new MemoryStream(bytes, 0, (int)readSize);
                var decompressed = GZipHelper.Decompress(stream);
                var deserialized = GodotBinaryConverter.Deserialize(decompressed);

                if (log)
                {
                    logger.LogDebug("received packet from {SteamId} {Channel} {Packet}", steamId, channel, deserialized);   
                }
                
                packetHandleManager.OnPacketReceived(steamId, channel, deserialized);

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "failed to packet processing");
                break;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }
        }
    }
}