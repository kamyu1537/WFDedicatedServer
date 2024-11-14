using System.Collections.Concurrent;
using System.Drawing;
using Microsoft.Extensions.Logging;
using Steamworks;
using WFDS.Common.Types.Manager;

namespace WFDS.Common.Types;

public interface IGameSession
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