using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Server.Core;

namespace WFDS.Server.EventHandler;

public class PlayerJoinTitleUpdateHandler(ILobbyManager lobby, ISessionManager session) : GameEventHandler<PlayerJoinedEvent>
{
    protected override void Handle(PlayerJoinedEvent e)
    {
        WebFishingServer.UpdateConsoleTitle(lobby.GetName(), lobby.GetCode(), session.GetSessionCount(), lobby.GetCap());
    }
}