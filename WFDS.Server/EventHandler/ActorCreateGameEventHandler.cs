using Steamworks;
using WFDS.Common.Actor;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.EventHandler;

internal class ActorCreateGameEventHandler(IActorManager actorManager, ISessionManager sessionManager) : GameEventHandler<ActorCreateEvent>
{
    protected override void Handle(ActorCreateEvent e)
    {
        var actor = actorManager.GetActor(e.ActorId);
        if (actor == null) return;

        if (actor.CreatorId == SteamClient.SteamId)
        {
            sessionManager.BroadcastP2PPacket(NetChannel.GameState, InstanceActorPacket.Create(actor));
            sessionManager.BroadcastP2PPacket(NetChannel.ActorAction, ActorActionPacket.CreateSetZonePacket(actor.ActorId, actor.Zone, actor.ZoneOwner));
            sessionManager.BroadcastP2PPacket(NetChannel.ActorUpdate, ActorUpdatePacket.Create(actor));
        }
    }
}