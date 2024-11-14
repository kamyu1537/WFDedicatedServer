using System.Numerics;
using WFDS.Common.ChannelEvents;
using WFDS.Common.Extensions;
using WFDS.Common.Types.Manager;
using WFDS.Server.Actors;

namespace WFDS.Server.EventHandlers;

public class RainCloudUpdateHandler(IActorManager actorManager) : ChannelEventHandler<ActorTickEvent>
{
    protected override async Task HandleAsync(ActorTickEvent e)
    {
        var delta = e.DeltaTime;

        var actor = actorManager.GetActor(e.ActorId);
        if (actor is not RainCloudActor rainCloud) return;
        if (rainCloud.IsDeadActor) return;
        
        var vel = new Vector2(1, 0).Rotated(rainCloud.Direction) * rainCloud.Speed;
        rainCloud.Position += new Vector3(vel.X, 0f, vel.X) * (float)delta;
        await Task.Yield();
    }
}

public class RainCloudCreatedHandler(IActorManager actorManager) : ChannelEventHandler<ActorCreateEvent>
{
    protected override async Task HandleAsync(ActorCreateEvent e)
    {
        var actor = actorManager.GetActor(e.ActorId);
        if (actor is not RainCloudActor rainCloud) return;
        
        var center = Vector3.Normalize(rainCloud.Position - new Vector3(30, 40, -50));
        rainCloud.Direction = new Vector2(center.X, center.Z).Angle();
        await Task.Yield();
    }
}