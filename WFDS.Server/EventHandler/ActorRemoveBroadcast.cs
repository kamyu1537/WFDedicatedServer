using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;

namespace WFDS.Server.EventHandler;

public sealed class ActorRemoveBroadcast(SessionManager sessionManager, SteamManager steam) : GameEventHandler<ActorRemoveEvent>
{
    protected override void Handle(ActorRemoveEvent e)
    {
        if (e.OwnerId != steam.SteamId) return;

        var queue = ActorActionPacket.CreateQueueFreePacket(e.ActorId);
        sessionManager.BroadcastPacket(NetChannel.ActorAction, queue);
    }
}