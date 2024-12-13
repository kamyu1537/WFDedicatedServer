using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Server.Core;

namespace WFDS.Server.EventHandler;

public class PlayerLeaveLog(PlayerLogManager playerLogManager) : GameEventHandler<PlayerLeaveEvent>
{
    protected override void Handle(PlayerLeaveEvent e)
    {
        playerLogManager.Append(e.PlayerId, e.DisplayName, "player_leave", "", null);
    }
}