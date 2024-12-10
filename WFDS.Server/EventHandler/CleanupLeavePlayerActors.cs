using WFDS.Common.Actor;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using ZLogger;

namespace WFDS.Server.EventHandler;

public class CleanupLeavePlayerActors(ILogger<CleanupLeavePlayerActors> logger, IActorManager actorManager) : GameEventHandler<PlayerLeaveEvent>
{
    protected override void Handle(PlayerLeaveEvent e)
    {
        logger.ZLogInformation($"player {e.PlayerId} has left the game");

        var actors = actorManager.GetActorsByCreatorId(e.PlayerId);
        foreach (var actor in actors)
        {
            logger.ZLogInformation($"try remove actor: {actor.ActorId}");
            if (actorManager.TryRemoveActor(actor.ActorId, ActorRemoveTypes.Disconnect, out _)) logger.LogDebug("removed actor: {ActorId}", actor.ActorId);
        }
    }
}