using WFDS.Common.Actor;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;

namespace WFDS.Server.EventHandler;

public sealed class UpdatePlayerHeldItem(IActorManager actorManager) : GameEventHandler<PlayerHeldItemUpdateEvent>
{
    protected override void Handle(PlayerHeldItemUpdateEvent e)
    {
        var actor = actorManager.GetPlayerActor(e.PlayerId);
        if (actor is null) return;

        actor.HeldItem = e.Item;
    }
}