using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Database;
using WFDS.Database.DbSet;
using ZLogger;

namespace WFDS.Server.EventHandler;

public class UpdateBannedPlayer(DataDbContext dbContext, ILogger<UpdateBannedPlayer> logger) : GameEventHandler<PlayerBanEvent>
{
    protected override void Handle(PlayerBanEvent e)
    {
        var playerName = Steamworks.SteamFriends.GetFriendPersonaName(e.PlayerId);
        if (playerName is null)
        {
            logger.ZLogWarning($"player name not found : {e.PlayerId}");
            playerName = string.Empty;
        }
        
        logger.ZLogInformation($"player {playerName}[{e.PlayerId}] has been banned");
        var bannedPlayer = dbContext.BannedPlayers.FirstOrDefault(x => x.SteamId == e.PlayerId.m_SteamID);
        if (bannedPlayer is not null)
        {
            logger.ZLogInformation($"player {playerName}[{e.PlayerId}] is already banned");
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