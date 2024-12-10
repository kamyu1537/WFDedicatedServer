using System.Numerics;
using WFDS.Common.Actor;
using WFDS.Common.Actor.Actors;
using WFDS.Common.Extensions;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;

namespace WFDS.Server.EventHandler;

public class RainCloudMove(IActorManager actorManager) : GameEventHandler<ActorTickEvent>
{
    protected override void Handle(ActorTickEvent e)
    {
        var delta = e.DeltaTime;

        var actor = actorManager.GetActor(e.ActorId);
        if (actor is not RainCloudActor rainCloud) return;
        if (rainCloud.IsDead || rainCloud.IsRemoved) return;

        var vel = new Vector2(1, 0).Rotated(rainCloud.Direction) * RainCloudActor.Speed;
        rainCloud.Position += new Vector3(vel.X, 0f, vel.X) * (float)delta;
    }
}