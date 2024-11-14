using Steamworks;
using WFDS.Common.Network;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Network;

public abstract class PacketHandler<T> : IPacketHandler where T : IPacket, new()
{
    private ISessionManager SessionManager { get; set; } = null!;
    
    public void Initialize(ISessionManager sessionManager)
    {
        SessionManager = sessionManager;
    }
    
    protected abstract Task HandlePacketAsync(IGameSession sender, NetChannel channel, T packet);

    public async Task HandlePacketAsync(IGameSession sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = PacketHelper.FromDictionary<T>(data);
        await HandlePacketAsync(sender, channel, packet);
    }
    
    public void SendP2PPacket(SteamId target, NetChannel channel, IPacket packet)
    {
        SessionManager.SendP2PPacket(target, channel, packet);
    }

    public void BroadcastP2PPacket(NetChannel channel, IPacket packet)
    {
        SessionManager.BroadcastP2PPacket(channel, packet);
    }
}