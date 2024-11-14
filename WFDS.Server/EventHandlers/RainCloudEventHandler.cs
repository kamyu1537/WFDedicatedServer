using System.Numerics;
using WFDS.Common.ActorEvents;
using WFDS.Common.Extensions;
using WFDS.Server.Actors;

namespace WFDS.Server.EventHandlers;

public class RainCloudUpdateHandler : ActorEventHandler<ActorTickEvent>
{
    protected override async Task HandleAsync(ActorTickEvent e)
    {
        var delta = e.DeltaTime;

        if (Actor.IsDeadActor) return;
        if (Actor is not RainCloudActor rainCloud) return;
        
        var vel = new Vector2(1, 0).Rotated(rainCloud.Direction) * rainCloud.Speed;
        Actor.Position += new Vector3(vel.X, 0f, vel.X) * (float)delta;
        await Task.Yield();
    }
}

public class RainCloudCreatedHandler : ActorEventHandler<ActorCreateEvent>
{
    protected override async Task HandleAsync(ActorCreateEvent e)
    {
        if (Actor is not RainCloudActor rainCloud) return;
        
        var center = Vector3.Normalize(rainCloud.Position - new Vector3(30, 40, -50));
        rainCloud.Direction = new Vector2(center.X, center.Z).Angle();
        await Task.Yield();
    }
}