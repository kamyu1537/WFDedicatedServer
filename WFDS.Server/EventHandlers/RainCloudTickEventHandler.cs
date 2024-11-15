using System.Numerics;
using WFDS.Common.Actor;
using WFDS.Common.Actor.Actors;
using WFDS.Common.ChannelEvents;
using WFDS.Common.ChannelEvents.Events;
using WFDS.Common.Extensions;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.EventHandlers;

public class RainCloudTickEventHandler(IActorManager actorManager) : ChannelEventHandler<ActorTickEvent>
{
    protected override async Task HandleAsync(ActorTickEvent e)
    {
        var delta = e.DeltaTime;

        var actor = actorManager.GetActor(e.ActorId);
        if (actor is not RainCloudActor rainCloud) return;
        if (rainCloud.IsDeadActor) return;
        
        var vel = new Vector2(1, 0).Rotated(rainCloud.Direction) * RainCloudActor.Speed;
        rainCloud.Position += new Vector3(vel.X, 0f, vel.X) * (float)delta;
        await Task.Yield();
    }
}