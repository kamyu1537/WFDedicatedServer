using System.Numerics;
using WFDS.Common.Actor;
using WFDS.Common.Actor.Actors;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;

namespace WFDS.Server.EventHandler;

internal class AmbientBirdTickGameEventHandler(ILogger<AmbientBirdTickGameEventHandler> logger, IActorManager actorManager) : GameEventHandler<ActorTickEvent>
{
    protected override void Handle(ActorTickEvent e)
    {
        var actor = actorManager.GetActor(e.ActorId);
        if (actor is not AmbientBirdActor bird) return;

        var players = actorManager.GetPlayerActors();
        foreach (var player in players)
        {
            var distance = Vector3.Distance(bird.Position, player.Position);
            if (distance <= 10)
            {
                logger.LogInformation("bird {ActorId} is near player {PlayerId}", actor.ActorId, player.ActorId);
                actorManager.TryRemoveActor(actor.ActorId, ActorRemoveTypes.QueueFree, out _);
                break;
            }
        }
    }
}