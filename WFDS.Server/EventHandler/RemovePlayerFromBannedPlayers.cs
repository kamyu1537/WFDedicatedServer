using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Database;


namespace WFDS.Server.EventHandler;

public class RemovePlayerFromBannedPlayers(ILogger<RemovePlayerFromBannedPlayers> logger, DatabaseContext dbContext) : GameEventHandler<PlayerUnBanEvent>
{
    protected override void Handle(PlayerUnBanEvent e)
    {
        var player = dbContext.BannedPlayers.FirstOrDefault(x => x.SteamId == e.PlayerId.m_SteamID);
        if (player is null)
        {
            logger.LogWarning("Player {SteamId} is not banned", e.PlayerId.m_SteamID);
            return;
        }
        
        dbContext.BannedPlayers.Remove(player);
        dbContext.SaveChanges();
    }
}