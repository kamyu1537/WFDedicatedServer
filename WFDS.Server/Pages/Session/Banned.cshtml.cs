using Microsoft.AspNetCore.Mvc.RazorPages;
using WFDS.Common.Steam;
using WFDS.Database;
using WFDS.Database.DbSet;

namespace WFDS.Server.Pages.Session;

public class Banned(DataDbContext dbContext) : PageModel
{
    public int GetBannedPlayerCount() => dbContext.BannedPlayers.Count();
    public IEnumerable<BannedPlayer> GetBannedPlayers() => dbContext.BannedPlayers;
    
    public void OnGet()
    {
        
    }
}