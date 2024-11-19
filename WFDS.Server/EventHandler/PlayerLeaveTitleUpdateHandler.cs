using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Server.Core;

namespace WFDS.Server.EventHandler;

public class PlayerLeaveTitleUpdateHandler(ILobbyManager lobby, ISessionManager session) : GameEventHandler<PlayerLeaveEvent>
{
    protected override void Handle(PlayerLeaveEvent e)
    {
        WebFishingServer.UpdateConsoleTitle(lobby.GetName(), lobby.GetCode(), session.GetSessionCount(), lobby.GetCap());
    }
}