using WFDS.Common.Types;

namespace WFDS.Common.Network;

public abstract class PacketHandler
{
    internal PacketHandler()
    {
    }
    
    public abstract Task HandlePacketAsync(Session sender, NetChannel channel, Dictionary<object, object> data);
}

public abstract class PacketHandler<T> : PacketHandler where T : Packet, new()
{
    protected abstract Task HandlePacketAsync(Session sender, NetChannel channel, T packet);

    public override async Task HandlePacketAsync(Session sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = PacketHelper.FromDictionary<T>(data);
        await HandlePacketAsync(sender, channel, packet);
    }
}