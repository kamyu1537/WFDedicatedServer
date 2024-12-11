using WFDS.Common.Actor;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;


namespace WFDS.Server.EventHandler;

public sealed class CleanupLeavePlayerActors(ILogger<CleanupLeavePlayerActors> logger, IActorManager actorManager) : GameEventHandler<PlayerLeaveEvent>
{
    protected override void Handle(PlayerLeaveEvent e)
    {
        logger.LogInformation("player {PlayerId} has left the game", e.PlayerId);

        var actors = actorManager.GetActorsByCreatorId(e.PlayerId);
        foreach (var actorId in actors.Select(x => x.ActorId))
        {
            logger.LogInformation("try remove actor: {ActorId}", actorId);
            if (actorManager.TryRemoveActor(actorId, ActorRemoveTypes.Disconnect, out _)) logger.LogDebug("removed actor: {ActorId}", actorId);
        }
    }
}