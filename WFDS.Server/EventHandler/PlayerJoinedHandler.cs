using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Helpers;
using WFDS.Common.Steam;
using WFDS.Database;
using WFDS.Database.DbSet;

namespace WFDS.Server.EventHandler;

public class PlayerJoinedHandler(LobbyManager lobby, SessionManager session, DataDbContext dbContext) : GameEventHandler<PlayerJoinedEvent>
{
    protected override void Handle(PlayerJoinedEvent e)
    {
        ConsoleHelper.UpdateConsoleTitle(lobby.GetName(), lobby.GetCode(), session.GetSessionCount(), lobby.GetCap());

        var playerName = Steamworks.SteamFriends.GetFriendPersonaName(e.PlayerId);
        if (playerName is null)
        {
            return;
        }

        var player = dbContext.Players.FirstOrDefault(x => x.SteamId == e.PlayerId.m_SteamID);
        if (player is null)
        {
            player = new Player()
            {
                SteamId = e.PlayerId.m_SteamID,
                DisplayName = playerName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastJoinedAt = DateTime.UtcNow
            };

            dbContext.Players.Add(player);
        }
        else
        {
            player.DisplayName = playerName;
            player.LastJoinedAt = DateTime.UtcNow;
            player.UpdatedAt = DateTime.UtcNow;
        }

        dbContext.SaveChanges();
    }
}