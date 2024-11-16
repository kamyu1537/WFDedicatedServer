using WFDS.Common.Actor;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;

namespace WFDS.Server.EventHandler;

internal class ActorTransformUpdateGameEventHandler(IActorManager actorManager) : GameEventHandler<ActorTransformUpdateEvent>
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