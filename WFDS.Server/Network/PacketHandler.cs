using Steamworks;
using WFDS.Common.Network;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.Network;

public abstract class PacketHandler<T> : IPacketHandler where T : IPacket, new()
{
    private IGameSessionManager SessionManager { get; set; } = null!;
    
    public void Initialize(IGameSessionManager sessionManager)
    {
        SessionManager = sessionManager;
    }
    
    protected abstract Task HandlePacketAsync(IGameSession sender, NetChannel channel, T packet);

    public async Task HandlePacketAsync(IGameSession sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = PacketHelper.FromDictionary<T>(data);
        await HandlePacketAsync(sender, channel, packet);
    }
    
    public void SendP2PPacket(SteamId target, NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1)
    {
        SessionManager.SendP2PPacket(target, channel, packet, zone, zoneOwner);
    }

    public void BroadcastP2PPacket(NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1)
    {
        SessionManager.BroadcastP2PPacket(channel, packet, zone, zoneOwner);
    }
}