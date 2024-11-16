using System.Numerics;
using WFDS.Common.Actor;
using WFDS.Common.Actor.Actors;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Extensions;

namespace WFDS.Server.EventHandler;

internal class RainCloudCreateGameEventHandler(IActorManager actorManager) : GameEventHandler<ActorCreateEvent>
{
    protected override async Task HandleAsync(ActorCreateEvent e)
    {
        var actor = actorManager.GetActor(e.ActorId);
        if (actor is not RainCloudActor rainCloud) return;
        
        var center = Vector3.Normalize(rainCloud.Position - new Vector3(30, 40, -50));
        rainCloud.Direction = new Vector2(center.X, center.Z).Angle();
        await Task.CompletedTask;
    }
}