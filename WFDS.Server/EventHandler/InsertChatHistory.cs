using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Steam;
using WFDS.Database;
using WFDS.Database.DbSet;
using WFDS.Server.Core;

namespace WFDS.Server.EventHandler;

public sealed class InsertChatHistory(SessionManager sessionManager, PlayerLogManager playerLogManager) : GameEventHandler<PlayerMessageEvent>
{
    protected override void Handle(PlayerMessageEvent e)
    {
        var session = sessionManager.GetSession(e.PlayerId);
        if (session is null) return;
        
        playerLogManager.Append(session, "message", e.Message, new { is_local = e.Local, color = e.Color });
    }
}