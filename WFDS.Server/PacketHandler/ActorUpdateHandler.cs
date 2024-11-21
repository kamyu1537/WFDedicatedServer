using WFDS.Common.Actor;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Server.Core.GameEvent;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("actor_update")]
public class ActorUpdateHandler(IActorManager actorManager) : PacketHandler<ActorUpdatePacket>
{
    protected override void Handle(Session sender, NetChannel channel, ActorUpdatePacket packet)
    {
        var actor = actorManager.GetActor(packet.ActorId);
        if (actor == null) return;

        if (actor.CreatorId != sender.SteamId) return;

        GameEventBus.Publish(new ActorTransformUpdateEvent(actor.ActorId, packet.Position, packet.Rotation));
    }
}