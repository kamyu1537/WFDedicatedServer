using Steamworks;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Steam;
using WFDS.Database;
using ZLogger;

namespace WFDS.Server.EventHandler;

public class LoadBanListFromDb(ILogger<LoadBanListFromDb> logger, DatabaseContext dbContext, SessionManager sessionManager) : GameEventHandler<LobbyCreatedEvent>
{
    protected override void Handle(LobbyCreatedEvent e)
    {
        var count = dbContext.BannedPlayers.Count();
        logger.ZLogInformation($"Load {count} banned players from database");
        
        if (count == 0) return;
        foreach (var banned in dbContext.BannedPlayers)
        {
            logger.ZLogInformation($"Banned player {banned.DisplayName}[{banned.SteamId}] is banned");
            sessionManager.BanPlayerNoEvent(e.LobbyId, new CSteamID(banned.SteamId));
        }
    }
}