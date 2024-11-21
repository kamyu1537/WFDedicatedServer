using WFDS.Common.Network;
using WFDS.Common.Types;
using WFDS.Server.Core.Network;

namespace WFDS.Common.Actor;

public static class ActorExtensions
{
    public static bool IsInZone(this IActor actor, string zone, long zoneOwner)
    {
        if (string.IsNullOrEmpty(zone))
        {
            return true;
        }

        var actorZone = actor.Zone;
        var actorZoneOwner = actor.ZoneOwner;

        return actorZone == zone && (zoneOwner == -1 || actorZoneOwner == zoneOwner);
    }
    
    public static void BroadcastInZone(this IActor actor, NetChannel channel, Packet packet, IActorManager actorManager, SessionManager session)
    {
        var players = actorManager.GetPlayerActors().ToArray();
        if (players.Length == 0) return;
        
        foreach (var player in players)
        {
            if (player.IsInZone(actor.Zone, actor.ZoneOwner))
            {
                session.SendP2PPacket(player.CreatorId, channel, packet);
            }
        }
    }
}