using Microsoft.Extensions.Logging;
using Steamworks;
using WFDS.Common.Types.Manager;

namespace WFDS.Common.Types;

public interface IPacketHandler
{
    IActorManager? ActorManager { get; set; }
    ISessionManager? SessionManager { get; set; }

    IPacketHandler Initialize(string packetType, ISessionManager sessionManager, IActorManager actorManager, ILogger logger);
    void HandlePacket(ISession sender, NetChannel channel, Dictionary<object, object> data);

    void SendP2PPacket(SteamId target, NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1);
    void BroadcastP2PPacket(NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1);
}