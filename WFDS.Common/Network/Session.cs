using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Steamworks;
using WFDS.Common.Helpers;
using WFDS.Common.Steam;
using WFDS.Common.Types;

namespace WFDS.Common.Network;

public sealed class Session(CSteamID steamId)
{
    private ILogger Logger { get; } = Log.Factory.CreateLogger<Session>();
    public CSteamID SteamId { get; init; } = steamId;
    public string Name { get; set; } = SteamFriends.GetFriendPersonaName(steamId);
    public bool HandshakeReceived { get; set; }
    public SteamNetworkingIdentity Identity { get; set; }

    public DateTimeOffset ConnectTime { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset HandshakeReceiveTime { get; set; }
    public DateTimeOffset PingReceiveTime { get; set; }
    public DateTimeOffset PacketReceiveTime { get; set; }

    public ConcurrentQueue<(NetChannel, object)> Packets { get; } = [];
    
    public override string ToString()
    {
        return $"{Name} ({SteamId})";
    }
    
    public void ProcessPacket()
    {
        if (Packets.TryDequeue(out var packet))
        {
            SessionManager.Inst.SendPacketObject(SteamId, packet.Item1, packet.Item2, false);
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
            SessionManager.Inst.SendPacketObject(SteamId, packet.Item1, packet.Item2, false);
        }
    }
}