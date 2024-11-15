using Steamworks;
using WFDS.Common.Actor;
using WFDS.Common.ChannelEvents;
using WFDS.Common.ChannelEvents.Events;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.EventHandler;

internal class ActorCreateEventHandler(IActorManager actorManager, ISessionManager sessionManager) : ChannelEventHandler<ActorCreateEvent>
{
    protected override async Task HandleAsync(ActorCreateEvent e)
    {
        var actor = actorManager.GetActor(e.ActorId);
        if (actor == null) return;
       
        if (actor.CreatorId == SteamClient.SteamId)
        {
            sessionManager.BroadcastP2PPacket(NetChannel.GameState, InstanceActorPacket.Create(actor));
            sessionManager.BroadcastP2PPacket(NetChannel.ActorAction, ActorActionPacket.CreateSetZonePacket(actor.ActorId, actor.Zone, actor.ZoneOwner));
            sessionManager.BroadcastP2PPacket(NetChannel.ActorUpdate, ActorUpdatePacket.Create(actor));
        }
        
        await Task.Yield();
    }
}