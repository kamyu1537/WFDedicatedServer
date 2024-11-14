using System.Numerics;
using WFDS.Common.ActorEvents;
using WFDS.Common.Types.Manager;
using WFDS.Server.Actors;

namespace WFDS.Server.EventHandlers;

public class AmbientBirdTickEventHandler(ILogger<AmbientBirdTickEventHandler> logger, IActorManager actorManager) : ActorEventHandler<ActorTickEvent>
{
    protected override async Task HandleAsync(ActorTickEvent e)
    {
        if (Actor is not AmbientBirdActor bird) return;

        var players = actorManager.GetPlayerActors();
        foreach (var player in players)
        {
            var distance = Vector3.Distance(bird.Position, player.Position);
            if (distance < 10)
            {
                logger?.LogInformation("bird {ActorId} is near player {PlayerId}", Actor.ActorId, player.ActorId);
                bird.DecayTimer = 0;
                break;
            }
        }
        
        await Task.Yield();
    }
}