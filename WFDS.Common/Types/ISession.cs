using System.Collections.Concurrent;
using System.Drawing;
using Microsoft.Extensions.Logging;
using Steamworks;
using WFDS.Common.Types.Manager;

namespace WFDS.Common.Types;

public interface ISession : IDisposable
{
    ISessionManager? SessionManager { get; set; }
    ILogger? Logger { get; set; }

    bool Disposed { get; set; }

    Friend Friend { get; set; }
    SteamId SteamId { get; set; }

    IPlayerActor? Actor { get; set; }
    bool ActorCreated { get; set; }

    bool HandshakeReceived { get; set; }
    DateTimeOffset ConnectTime { get; set; }
    DateTimeOffset HandshakeReceiveTime { get; set; }
    DateTimeOffset PingReceiveTime { get; set; }
    DateTimeOffset PacketReceiveTime { get; set; }

    ConcurrentQueue<(NetChannel, byte[])> Packets { get; }

    void SendP2PPacket(NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1);
    void SendMessage(string message, Color color, bool local = false);
    void SendLetter(SteamId target, string body, List<GameItem> items);
    void Kick();
    void TempBan();
    void ServerClose();

    void ProcessPacket();
}