using WFDS.Common.Actor;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Steam;
using WFDS.Server.Core;

namespace WFDS.Server.EventHandler;

public sealed class UpdateActorZone(IActorManager actorManager, SessionManager sessionManager, PlayerLogManager playerLogManager) : GameEventHandler<ActorZoneUpdateEvent>
{
    protected override void Handle(ActorZoneUpdateEvent e)
    {
        var actor = actorManager.GetActor(e.ActorId);
        if (actor is null) return;

        if (actor.Type == ActorType.Player && (e.ZoneOwner != actor.ZoneOwner || e.Zone != actor.Zone))
        {
            var session = sessionManager.GetSession(actor.CreatorId);
            if (session is not null)
            {
                playerLogManager.Append(session, "zone_change", $"{e.Zone}[{e.ZoneOwner}]", new
                {
                    prev_zone = actor.Zone,
                    prev_zone_owner = actor.ZoneOwner,
                    new_zone = e.Zone,
                    new_zone_owner = e.ZoneOwner,
                });   
            }
        }

        actor.Zone = e.Zone;
        actor.ZoneOwner = e.ZoneOwner;
    }
}