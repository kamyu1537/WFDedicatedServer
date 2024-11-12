using System.Collections.Concurrent;
using System.Text.Json;
using Steamworks;
using WFDS.Common.Extensions;
using WFDS.Common.Helpers;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Godot.Binary;
using WFDS.Godot.Types;
using WFDS.Server.Packets;
using Color = System.Drawing.Color;

namespace WFDS.Server.Network;

public sealed class Session(ISessionManager sessionManager, ILogger logger) : ISession
{
    public ISessionManager SessionManager { get; } = sessionManager;
    public ILogger Logger { get; } = logger;

    public bool Disposed { get; set; }

    public Friend Friend { get; set; }
    public SteamId SteamId { get; set; }

    public IPlayerActor? Actor { get; set; }
    public bool ActorCreated { get; set; }

    public bool HandshakeReceived { get; set; }

    public DateTimeOffset ConnectTime { get; set; }
    public DateTimeOffset HandshakeReceiveTime { get; set; }
    public DateTimeOffset PingReceiveTime { get; set; }
    public DateTimeOffset PacketReceiveTime { get; set; }

    public ConcurrentQueue<(NetChannel, byte[])> Packets { get; } = [];

    public void SendP2PPacket(NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1)
    {
        SessionManager.SendP2PPacket(SteamId, channel, packet, zone, zoneOwner);
    }

    public void SendMessage(string message, Color color, bool local = false)
    {
        SessionManager.SendP2PPacket(SteamId, NetChannel.GameState, new MessagePacket
        {
            Message = message,
            Color = color.ToHex(true),
            Local = local,
            Position = Vector3.Zero,
            Zone = "",
            ZoneOwner = -1
        });
    }

    public void SendLetter(SteamId target, string body, List<GameItem> items)
    {
        SessionManager.SendP2PPacket(SteamId, NetChannel.GameState, new LetterReceivedPacket
        {
            To = target.Value.ToString(),
            Data = new LetterData
            {
                LetterId = new Random().Next(),
                To = SteamClient.SteamId.ToString(),
                From = SteamClient.SteamId.ToString(),
                Closing = "From, ",
                User = "[SERVER]",
                Header = "Letter",
                Body = body,
                Items = []
            }
        });
    }

    public void Kick()
    {
        ClearPacketQueue();
        SessionManager.KickPlayer(SteamId);
        ProcessPackets();
    }

    public void TempBan()
    {
        ClearPacketQueue();
        SessionManager.TempBanPlayer(SteamId);
        ProcessPackets();
    }

    public void ServerClose()
    {
        ClearPacketQueue();
        SessionManager.ServerClose(SteamId);
        ProcessPackets();
    }

    public void ProcessPacket()
    {
        if (Disposed) return;

        if (Packets.TryDequeue(out var packet))
        {
#if DEBUG
            // Console.WriteLine(JsonSerializer.Serialize(GodotBinaryConverter.Deserialize(GZipHelper.Decompress(packet.Item2))));
#endif
            SteamNetworking.SendP2PPacket(SteamId, packet.Item2, nChannel: packet.Item1.Value);
        }
    }

    private void ClearPacketQueue()
    {
        Packets.Clear();
    }

    private void ProcessPackets()
    {
        if (Disposed) return;

        while (Packets.TryDequeue(out var packet))
        {
#if DEBUG
            // Console.WriteLine(JsonSerializer.Serialize(GodotBinaryConverter.Deserialize(GZipHelper.Decompress(packet.Item2))));
#endif
            SteamNetworking.SendP2PPacket(SteamId, packet.Item2, nChannel: packet.Item1.Value);
        }
    }

    public void Dispose()
    {
        if (Disposed) return;
        Disposed = true;

        Packets.Clear();
        Actor = null;
    }
}