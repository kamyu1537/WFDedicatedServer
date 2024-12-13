using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Server.Core;

namespace WFDS.Server.EventHandler;

public class PlayerJoinLog(PlayerLogManager playerLogManager) : GameEventHandler<PlayerJoinedEvent>
{
    protected override void Handle(PlayerJoinedEvent e)
    {
        playerLogManager.Append(e.PlayerId, e.DisplayName, "player_join", "", null);
    }
}