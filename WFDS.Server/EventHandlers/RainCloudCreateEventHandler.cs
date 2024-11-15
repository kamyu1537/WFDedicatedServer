using System.Numerics;
using WFDS.Common.Actor;
using WFDS.Common.Actor.Actors;
using WFDS.Common.ChannelEvents;
using WFDS.Common.ChannelEvents.Events;
using WFDS.Common.Extensions;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.EventHandlers;

public class RainCloudCreateEventHandler(IActorManager actorManager) : ChannelEventHandler<ActorCreateEvent>
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