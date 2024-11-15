using WFDS.Common.Actor;
using WFDS.Common.ChannelEvents;
using WFDS.Common.ChannelEvents.Events;

namespace WFDS.Server.EventHandler;

internal class ActorZoneUpdateEventHandler(IActorManager actorManager) : ChannelEventHandler<ActorZoneUpdateEvent>
{
    protected override async Task HandleAsync(ActorZoneUpdateEvent e)
    {
        var actor = actorManager.GetActor(e.ActorId);
        if (actor is null) return;

        actor.Zone = e.Zone;
        actor.ZoneOwner = e.ZoneOwner;
        await Task.Yield();
    }
}