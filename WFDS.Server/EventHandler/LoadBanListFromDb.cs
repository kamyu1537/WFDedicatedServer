using Steamworks;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Steam;
using WFDS.Database;


namespace WFDS.Server.EventHandler;

public class LoadBanListFromDb(ILogger<LoadBanListFromDb> logger, DatabaseContext dbContext, SessionManager sessionManager) : GameEventHandler<LobbyCreatedEvent>
{
    protected override void Handle(LobbyCreatedEvent e)
    {
        var count = dbContext.BannedPlayers.Count();
        logger.LogInformation("load {Count} banned players from database", count);
        
        if (count == 0) return;
        foreach (var banned in dbContext.BannedPlayers)
        {
            logger.LogInformation("banned player {DisplayName}[{SteamId}] is banned", banned.DisplayName, banned.SteamId);
            sessionManager.BanPlayerNoEvent(e.LobbyId, new CSteamID(banned.SteamId));
        }
    }
}