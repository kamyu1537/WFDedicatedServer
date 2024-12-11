using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Database;
using WFDS.Database.DbSet;


namespace WFDS.Server.EventHandler;

public class UpdateBannedPlayer(DatabaseContext dbContext, ILogger<UpdateBannedPlayer> logger) : GameEventHandler<PlayerBanEvent>
{
    protected override void Handle(PlayerBanEvent e)
    {
        var playerName = Steamworks.SteamFriends.GetFriendPersonaName(e.PlayerId);
        if (playerName is null)
        {
            logger.LogWarning("player name not found : {PlayerId}", e.PlayerId);
            playerName = string.Empty;
        }

        logger.LogInformation("player {DisplayName}[{PlayerId}] has been banned", playerName, e.PlayerId);
        var bannedPlayer = dbContext.BannedPlayers.FirstOrDefault(x => x.SteamId == e.PlayerId.m_SteamID);
        if (bannedPlayer is not null)
        {
            logger.LogInformation("player {DisplayName}[{PlayerId}] is already banned", playerName, e.PlayerId);
            return;
        }

        bannedPlayer = new BannedPlayer
        {
            SteamId = e.PlayerId.m_SteamID,
            DisplayName = playerName,
            BannedAt = DateTime.UtcNow,
        };

        dbContext.BannedPlayers.Add(bannedPlayer);
        dbContext.SaveChanges();
    }
}