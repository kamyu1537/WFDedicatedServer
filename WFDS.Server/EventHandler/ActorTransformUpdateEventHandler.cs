using WFDS.Common.Actor;
using WFDS.Common.ChannelEvents;
using WFDS.Common.ChannelEvents.Events;

namespace WFDS.Server.EventHandler;

internal class ActorTransformUpdateEventHandler(IActorManager actorManager) : ChannelEventHandler<ActorTransformUpdateEvent>
{
    protected override async Task HandleAsync(ActorTransformUpdateEvent e)
    {
        var actor = actorManager.GetActor(e.ActorId);
        if (actor is null) return;

        actor.Position = e.Position;
        actor.Rotation = e.Rotation;
        await Task.Yield();
    }
}