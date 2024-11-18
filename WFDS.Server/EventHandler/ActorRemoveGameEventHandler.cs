using Steamworks;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.EventHandler;

public class ActorRemoveGameEventHandler(ISessionManager sessionManager, ILobbyManager lobby) : GameEventHandler<ActorRemoveEvent>
{
    protected override void Handle(ActorRemoveEvent e)
    {
        if (e.OwnerId != SteamUser.GetSteamID()) return;

        // var wipe = ActorActionPacket.CreateWipeActorPacket(e.ActorId);
        // sessionManager.BroadcastP2PPacket(NetChannel.ActorAction, wipe);

        var queue = ActorActionPacket.CreateQueueFreePacket(e.ActorId);
        sessionManager.BroadcastP2PPacket(lobby.GetLobbyId(), NetChannel.ActorAction, queue);
    }
}