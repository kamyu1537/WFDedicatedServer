using System.Collections.Concurrent;
using Steamworks;
using WFDS.Common.Types;

namespace WFDS.Network;

public sealed class GameSession : IGameSession
{
    public Friend Friend { get; set; }
    public SteamId SteamId { get; set; }
    public bool HandshakeReceived { get; set; }

    public DateTimeOffset ConnectTime { get; set; }
    public DateTimeOffset HandshakeReceiveTime { get; set; }
    public DateTimeOffset PingReceiveTime { get; set; }
    public DateTimeOffset PacketReceiveTime { get; set; }

    public ConcurrentQueue<(NetChannel, byte[])> Packets { get; } = [];

    public void ProcessPacket()
    {
        if (Packets.TryDequeue(out var packet))
        {
            SteamNetworking.SendP2PPacket(SteamId, packet.Item2, nChannel: packet.Item1.Value);
        }
    }

    public void ClearPacketQueue()
    {
        Packets.Clear();
    }

    public void ProcessPackets()
    {
        while (Packets.TryDequeue(out var packet))
        {
            SteamNetworking.SendP2PPacket(SteamId, packet.Item2, nChannel: packet.Item1.Value);
        }
    }
}