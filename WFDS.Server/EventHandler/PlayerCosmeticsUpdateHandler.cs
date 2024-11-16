using WFDS.Common.Actor;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;

namespace WFDS.Server.EventHandler;

internal class PlayerCosmeticsUpdateHandler(IActorManager actorManager) : GameEventHandler<PlayerCosmeticsUpdateEvent>
{
    protected override async Task HandleAsync(PlayerCosmeticsUpdateEvent e)
    {
        var actor = actorManager.GetPlayerActor(e.PlayerId);
        if (actor is null) return;

        actor.Cosmetics = e.Cosmetics;
        await Task.Yield();
    }
}