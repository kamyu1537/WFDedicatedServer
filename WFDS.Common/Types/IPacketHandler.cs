using Microsoft.Extensions.Logging;
using Steamworks;
using WFDS.Common.Types.Manager;

namespace WFDS.Common.Types;

public interface IPacketHandler
{
    public void Initialize(ISessionManager sessionManager);
    Task HandlePacketAsync(IGameSession sender, NetChannel channel, Dictionary<object, object> data);

    void SendP2PPacket(SteamId target, NetChannel channel, IPacket packet);
    void BroadcastP2PPacket(NetChannel channel, IPacket packet);
}