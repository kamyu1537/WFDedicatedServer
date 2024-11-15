using System.Numerics;
using WFDS.Common.Actor;
using WFDS.Common.Actor.Actors;
using WFDS.Common.ChannelEvents;
using WFDS.Common.ChannelEvents.Events;

namespace WFDS.Server.EventHandler;

internal class AmbientBirdTickEventHandler(ILogger<AmbientBirdTickEventHandler> logger, IActorManager actorManager) : ChannelEventHandler<ActorTickEvent>
{
    protected override async Task HandleAsync(ActorTickEvent e)
    {
        var actor = actorManager.GetActor(e.ActorId);
        if (actor is not AmbientBirdActor bird) return;

        var players = actorManager.GetPlayerActors();
        foreach (var player in players)
        {
            var distance = Vector3.Distance(bird.Position, player.Position);
            if (distance < 10)
            {
                logger?.LogInformation("bird {ActorId} is near player {PlayerId}", actor.ActorId, player.ActorId);
                bird.DecayTimer = 0;
                break;
            }
        }
        
        await Task.Yield();
    }
}