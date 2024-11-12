using WFDS.Common.Types;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("actor_update")]
public class ActorUpdateHandler : PacketHandler<ActorUpdatePacket>
{
    protected override void HandlePacket(ISession sender, NetChannel channel, ActorUpdatePacket packet)
    {
        ActorManager?.SelectActor(packet.ActorId, actor =>
        {
            actor.OnActorUpdated(packet.Position, packet.Rotation);
        });
    }
}