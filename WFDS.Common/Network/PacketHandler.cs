using Steamworks;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Common.Network;

public abstract class PacketHandler
{
    internal PacketHandler()
    {
    }
    
    public abstract void Initialize(ISessionManager sessionManager);
    public abstract Task HandlePacketAsync(ISession sender, NetChannel channel, Dictionary<object, object> data);

    protected abstract void SendP2PPacket(SteamId target, NetChannel channel, IPacket packet);
    protected abstract void BroadcastP2PPacket(NetChannel channel, IPacket packet);
}

public abstract class PacketHandler<T> : PacketHandler where T : IPacket, new()
{
    private ISessionManager SessionManager { get; set; } = null!;
    
    public override void Initialize(ISessionManager sessionManager)
    {
        SessionManager = sessionManager;
    }
    
    protected abstract Task HandlePacketAsync(ISession sender, NetChannel channel, T packet);

    public override async Task HandlePacketAsync(ISession sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = PacketHelper.FromDictionary<T>(data);
        await HandlePacketAsync(sender, channel, packet);
    }
    
    protected override void SendP2PPacket(SteamId target, NetChannel channel, IPacket packet)
    {
        SessionManager.SendP2PPacket(target, channel, packet);
    }

    protected override void BroadcastP2PPacket(NetChannel channel, IPacket packet)
    {
        SessionManager.BroadcastP2PPacket(channel, packet);
    }
}