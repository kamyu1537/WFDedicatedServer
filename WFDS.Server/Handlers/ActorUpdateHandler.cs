using WFDS.Common.Types;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("actor_update")]
public class ActorUpdateHandler : PacketHandler
{
    public override void HandlePacket(ISession sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = new ActorUpdatePacket();
        packet.Parse(data);

        ActorManager?.SelectActor(packet.ActorId, actor =>
        {
            actor.OnActorUpdated(packet.Position, packet.Rotation);
        });
    }
}