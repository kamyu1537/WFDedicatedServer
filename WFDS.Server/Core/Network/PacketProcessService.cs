using System.Buffers;
using System.Runtime.InteropServices;
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

    private bool ValidatePacket(NetChannel channel, NetworkMessage message)
    {
        var steamId = new CSteamID(message.Identity.GetSteamID64());
        if (sessionManager.IsBannedPlayer(steamId))
        {
            var packetDataBase64 = Convert.ToBase64String(message.Data);
            logger.LogError("banned player {SteamId} tried to send packet: {PacketBytes}", steamId, packetDataBase64);
            return false;
        }

        if (message.Data.Length != 0)
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

        var messages = SteamNetworkHelper.ReadMessagesOnChannel(channel, 8, out var count);
        for (var i = 0; i < count; i++)
        {
            var message = messages[i];
            if (message == null)
            {
                continue;
            }
            
            ProcessMessage(channel, message, log);
        }
    }

    /*
     *
     */

    private void ProcessMessage(NetChannel channel, NetworkMessage message, bool log)
    {
        try
        {
            if (!ValidatePacket(channel, message))
            {
                return;
            }

            var steamId = new CSteamID(message.Identity.GetSteamID64());
            using var stream = new MemoryStream(message.Data, 0, message.Data.Length);
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
        }
    }
}