using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Helpers;
using WFDS.Common.Steam;
using WFDS.Server.Core.Network;

namespace WFDS.Server.EventHandler;

public class PlayerLeaveTitleUpdateHandler(LobbyManager lobby, SessionManager session) : GameEventHandler<PlayerLeaveEvent>
{
    protected override void Handle(PlayerLeaveEvent e)
    {
        ConsoleHelper.UpdateConsoleTitle(lobby.GetName(), lobby.GetCode(), session.GetSessionCount(), lobby.GetCap());
    }
}