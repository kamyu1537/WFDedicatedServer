using WFDS.Common.Actor;
using WFDS.Common.ChannelEvents;
using WFDS.Common.ChannelEvents.Events;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.EventHandlers;

public class PlayerLeaveEventHandler(ILogger<PlayerLeaveEventHandler> logger, IActorManager actorManager) : ChannelEventHandler<PlayerLeaveEvent>
{
    protected override async Task HandleAsync(PlayerLeaveEvent e)
    {
        logger.LogInformation("player {PlayerId} has left the game", e.PlayerId);
        
        var actors = actorManager.GetActorsByCreatorId(e.PlayerId);
        foreach (var actor in actors)
        {
            logger.LogDebug("try remove actor: {ActorId}", actor.ActorId);
            if (actorManager.TryRemoveActor(actor.ActorId, ActorRemoveTypes.Disconnect, out _))
            {
                logger.LogDebug("removed actor: {ActorId}", actor.ActorId);
            }
        }
        
        await Task.Yield();
    }
}