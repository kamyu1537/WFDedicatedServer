using System.Collections.Concurrent;
using System.Text.Json;
using Steamworks;
using WFDS.Godot.Binary;
using WFDS.Godot.Types;
using WFDS.Server.Common.Actor;
using WFDS.Server.Common.Extensions;
using WFDS.Server.Common.Helpers;
using WFDS.Server.Managers;
using WFDS.Server.Packets;
using Color = System.Drawing.Color;

namespace WFDS.Server.Common;

public class Session : ISession
{
    public LobbyManager LobbyManager { get; set; } = null!;
    public Friend Friend { get; set; }
    public SteamId SteamId { get; set; }

    public PlayerActor Actor { get; set; } = null!;
    public bool ActorCreated { get; set; }

    public bool HandshakeReceived { get; set; }

    public DateTimeOffset ConnectTime { get; set; }
    public DateTimeOffset HandshakeReceiveTime { get; set; }
    public DateTimeOffset PingReceiveTime { get; set; }
    public DateTimeOffset PacketReceiveTime { get; set; }

    public ConcurrentQueue<(NetChannel, byte[])> Packets { get; } = [];

    public void SendPacket(NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1)
    {
        LobbyManager.SendPacket(SteamId, channel, packet, zone, zoneOwner);
    }

    public void SendMessage(string message, Color color, bool local = false)
    {
        SendPacket(NetChannel.GameState, new MessagePacket
        {
            Message = message,
            Color = color.ToHex(true),
            Local = local,
            Position = Vector3.Zero,
            Zone = "main_zone",
            ZoneOwner = -1
        });
    }

    public void SendLetter(SteamId target, string body)
    {
        SendPacket(NetChannel.GameState, new LetterReceivedPacket
        {
            LatterId = new Random().Next(),
            From = SteamClient.SteamId.ToString(),
            To = target.Value.ToString(),
            Closing = "From, ",
            User = "[SERVER]",
            Header = "Letter",
            Body = body,
            Items = []
        });
    }

    public void ProcessPackets()
    {
        if (Packets.TryDequeue(out var packet))
        {
            SteamNetworking.SendP2PPacket(SteamId, packet.Item2, nChannel: packet.Item1.Value);
        }
    }
}