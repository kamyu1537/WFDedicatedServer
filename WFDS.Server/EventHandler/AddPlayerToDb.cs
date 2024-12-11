using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Database;
using WFDS.Database.DbSet;

namespace WFDS.Server.EventHandler;

public class AddPlayerToDb(DatabaseContext dbContext) : GameEventHandler<PlayerJoinedEvent>
{
    protected override void Handle(PlayerJoinedEvent e)
    {
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
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                LastJoinedAt = DateTimeOffset.UtcNow
            };

            dbContext.Players.Add(player);
        }
        else
        {
            player.DisplayName = playerName;
            player.LastJoinedAt = DateTimeOffset.UtcNow;
            player.UpdatedAt = DateTimeOffset.UtcNow;
        }

        dbContext.SaveChanges();
    }
}