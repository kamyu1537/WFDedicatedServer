using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Helpers;
using WFDS.Common.Steam;
using WFDS.Server.Core;

namespace WFDS.Server.EventHandler;

public sealed class PlayerJoinUpdateTitle(LobbyManager lobby, SessionManager sessionManager) : GameEventHandler<PlayerJoinedEvent>
{
    protected override void Handle(PlayerJoinedEvent e)
    {
        ConsoleHelper.UpdateConsoleTitle(lobby.GetName(), lobby.GetCode(), sessionManager.GetSessionCount(), lobby.GetCap());
    }
}