using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Database;
using ZLogger;

namespace WFDS.Server.EventHandler;

public class RemovePlayerFromBannedPlayers(ILogger<RemovePlayerFromBannedPlayers> logger, DataDbContext dbContext) : GameEventHandler<PlayerUnBanEvent>
{
    protected override void Handle(PlayerUnBanEvent e)
    {
        var player = dbContext.BannedPlayers.FirstOrDefault(x => x.SteamId == e.PlayerId.m_SteamID);
        if (player is null)
        {
            logger.ZLogWarning($"Player {e.PlayerId.m_SteamID} is not banned");
            return;
        }
        
        dbContext.BannedPlayers.Remove(player);
        dbContext.SaveChanges();
    }
}