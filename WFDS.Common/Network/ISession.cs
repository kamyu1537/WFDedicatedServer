using System.Collections.Concurrent;
using Steamworks;

namespace WFDS.Common.Types;

public interface ISession
{ 
    Friend Friend { get; set; }
    SteamId SteamId { get; set; }
    
    bool HandshakeReceived { get; set; }
    DateTimeOffset ConnectTime { get; set; }
    DateTimeOffset HandshakeReceiveTime { get; set; }
    DateTimeOffset PingReceiveTime { get; set; }
    DateTimeOffset PacketReceiveTime { get; set; }

    ConcurrentQueue<(NetChannel, byte[])> Packets { get; }

    void ClearPacketQueue();
    void ProcessPacket();
    void ProcessPackets();
}