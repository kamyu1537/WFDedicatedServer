using Steamworks;
using WFDS.Common.ChannelEvents;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Network.Packets;

namespace WFDS.Server.EventHandlers;

public class ActorCreateHandler(IActorManager actorManager, ISessionManager sessionManager) : ChannelEventHandler<ActorCreateEvent>
{
    protected override async Task HandleAsync(ActorCreateEvent e)
    {
        var actor = actorManager.GetActor(e.ActorId);
        if (actor == null) return;
       
        if (actor.CreatorId == SteamClient.SteamId)
        {
            sessionManager.BroadcastP2PPacket(NetChannel.ActorAction, InstanceActorPacket.Create(actor));
            sessionManager.BroadcastP2PPacket(NetChannel.ActorAction, ActorUpdatePacket.Create(actor));
        }
        
        await Task.Yield();
    }
}

public class ActorZoneUpdateHandler(IActorManager actorManager) : ChannelEventHandler<ActorZoneUpdateEvent>
{
    protected override async Task HandleAsync(ActorZoneUpdateEvent e)
    {
        var actor = actorManager.GetActor(e.ActorId);
        if (actor is null) return;

        actor.Zone = e.Zone;
        actor.ZoneOwner = e.ZoneOwner;
        await Task.Yield();
    }
}

public class ActorUpdateEventHandler(IActorManager actorManager) : ChannelEventHandler<ActorUpdateEvent>
{
    protected override async Task HandleAsync(ActorUpdateEvent e)
    {
        var actor = actorManager.GetActor(e.ActorId);
        if (actor is null) return;

        actor.Position = e.Position;
        actor.Rotation = e.Rotation;
        await Task.Yield();
    }
}

public class ActorRemoveEventHandler(IActorManager actorManager, ISessionManager sessionManager) : ChannelEventHandler<ActorRemoveEvent>
{
    protected override async Task HandleAsync(ActorRemoveEvent e)
    {
        var actor = actorManager.GetActor(e.ActorId);
        if (actor == null) return;
        
        var wipe = ActorActionPacket.CreateWipeActorPacket(e.ActorId);
        sessionManager.BroadcastP2PPacket(NetChannel.ActorAction, wipe);
        
        if (actor.CreatorId == SteamClient.SteamId)
        {
            var queue = ActorActionPacket.CreateQueueFreePacket(e.ActorId);
            sessionManager.BroadcastP2PPacket(NetChannel.ActorAction, queue);
        }
        
        await Task.Yield();
    }
}