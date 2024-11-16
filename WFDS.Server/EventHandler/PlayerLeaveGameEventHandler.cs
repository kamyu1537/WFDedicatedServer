using WFDS.Common.Actor;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;

namespace WFDS.Server.EventHandler;

internal class PlayerLeaveGameEventHandler(ILogger<PlayerLeaveGameEventHandler> logger, IActorManager actorManager) : GameEventHandler<PlayerLeaveEvent>
{
    protected override void Handle(PlayerLeaveEvent e)
    {
        logger.LogInformation("player {PlayerId} has left the game", e.PlayerId);

        var actors = actorManager.GetActorsByCreatorId(e.PlayerId);
        foreach (var actor in actors)
        {
            logger.LogDebug("try remove actor: {ActorId}", actor.ActorId);
            if (actorManager.TryRemoveActor(actor.ActorId, ActorRemoveTypes.Disconnect, out _)) logger.LogDebug("removed actor: {ActorId}", actor.ActorId);
        }
    }
}