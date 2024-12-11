using WFDS.Common.Actor;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;

namespace WFDS.Server.EventHandler;

public sealed class UpdateActorZone(IActorManager actorManager) : GameEventHandler<ActorZoneUpdateEvent>
{
    protected override void Handle(ActorZoneUpdateEvent e)
    {
        var actor = actorManager.GetActor(e.ActorId);
        if (actor is null) return;

        actor.Zone = e.Zone;
        actor.ZoneOwner = e.ZoneOwner;
    }
}