using WFDS.Common.Actor;
using WFDS.Common.ChannelEvents;
using WFDS.Common.ChannelEvents.Events;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.EventHandlers;

public class PlayerCosmeticsUpdateHandler(IActorManager actorManager) : ChannelEventHandler<PlayerCosmeticsUpdateEvent>
{
    protected override async Task HandleAsync(PlayerCosmeticsUpdateEvent e)
    {
        var actor = actorManager.GetPlayerActor(e.PlayerId);
        if (actor is null) return;

        actor.Cosmetics = e.Cosmetics;
        await Task.Yield();
    }
}