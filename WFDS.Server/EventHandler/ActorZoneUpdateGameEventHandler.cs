using WFDS.Common.Actor;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;

namespace WFDS.Server.EventHandler;

internal class ActorZoneUpdateGameEventHandler(IActorManager actorManager) : GameEventHandler<ActorZoneUpdateEvent>
{
    protected override async Task HandleAsync(ActorZoneUpdateEvent e)
    {
        var actor = actorManager.GetActor(e.ActorId);
        if (actor is null) return;

        actor.Zone = e.Zone;
        actor.ZoneOwner = e.ZoneOwner;
        await Task.CompletedTask;
    }
}