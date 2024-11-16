using Steamworks;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.EventHandler;

internal class ActorRemoveGameEventHandler(ISessionManager sessionManager) : GameEventHandler<ActorRemoveEvent>
{
    protected override async Task HandleAsync(ActorRemoveEvent e)
    {
        if (e.OwnerId != SteamClient.SteamId) return;
        
        var wipe = ActorActionPacket.CreateWipeActorPacket(e.ActorId);
        sessionManager.BroadcastP2PPacket(NetChannel.ActorAction, wipe);
        
        var queue = ActorActionPacket.CreateQueueFreePacket(e.ActorId);
        sessionManager.BroadcastP2PPacket(NetChannel.ActorAction, queue);
        
        await Task.Yield();
    }
}