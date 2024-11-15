using Steamworks;
using WFDS.Common.Actor;
using WFDS.Common.ChannelEvents;
using WFDS.Common.ChannelEvents.Events;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.EventHandler;

internal class ActorRemoveEventHandler(IActorManager actorManager, ISessionManager sessionManager) : ChannelEventHandler<ActorRemoveEvent>
{
    protected override async Task HandleAsync(ActorRemoveEvent e)
    {
        var wipe = ActorActionPacket.CreateWipeActorPacket(e.ActorId);
        sessionManager.BroadcastP2PPacket(NetChannel.ActorAction, wipe);
        
        if (e.OwnerId == SteamClient.SteamId)
        {
            var queue = ActorActionPacket.CreateQueueFreePacket(e.ActorId);
            sessionManager.BroadcastP2PPacket(NetChannel.ActorAction, queue);
        }
        
        await Task.Yield();
    }
}