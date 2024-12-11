using WFDS.Common.Actor;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;

namespace WFDS.Server.EventHandler;

public sealed class UpdatePlayerCosmetics(IActorManager actorManager) : GameEventHandler<PlayerCosmeticsUpdateEvent>
{
    protected override void Handle(PlayerCosmeticsUpdateEvent e)
    {
        var actor = actorManager.GetPlayerActor(e.PlayerId);
        if (actor is null) return;

        actor.Cosmetics = e.Cosmetics;
    }
}