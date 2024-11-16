using System.Numerics;
using WFDS.Common.Actor;
using WFDS.Common.Actor.Actors;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Extensions;

namespace WFDS.Server.EventHandler;

internal class RainCloudTickGameEventHandler(IActorManager actorManager) : GameEventHandler<ActorTickEvent>
{
    protected override async Task HandleAsync(ActorTickEvent e)
    {
        var delta = e.DeltaTime;

        var actor = actorManager.GetActor(e.ActorId);
        if (actor is not RainCloudActor rainCloud) return;
        if (rainCloud.IsDead || rainCloud.IsRemoved) return;
        
        var vel = new Vector2(1, 0).Rotated(rainCloud.Direction) * RainCloudActor.Speed;
        rainCloud.Position += new Vector3(vel.X, 0f, vel.X) * (float)delta;
        await Task.CompletedTask;
    }
}