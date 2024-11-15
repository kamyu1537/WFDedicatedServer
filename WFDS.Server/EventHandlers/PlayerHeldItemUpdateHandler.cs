using WFDS.Common.Actor;
using WFDS.Common.ChannelEvents;
using WFDS.Common.ChannelEvents.Events;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.EventHandlers;

public class PlayerHeldItemUpdateHandler(IActorManager actorManager) : ChannelEventHandler<PlayerHeldItemUpdateEvent>
{
    protected override async Task HandleAsync(PlayerHeldItemUpdateEvent e)
    {
        var actor = actorManager.GetPlayerActor(e.PlayerId);
        if (actor is null) return;

        actor.HeldItem = e.Item;
        await Task.Yield();
    }
}