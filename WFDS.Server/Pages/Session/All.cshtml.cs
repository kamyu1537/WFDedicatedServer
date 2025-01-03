using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Steamworks;
using WFDS.Common.Steam;
using WFDS.Database;
using WFDS.Database.DbSet;

namespace WFDS.Server.Pages.Session;

public class All(DatabaseContext dbContext, SessionManager sessionManager, LobbyManager lobbyManager) : PageModel
{
    public void OnGet()
    {
        
    }
    
    public long GetTotalPlayerCount()
    {
        return dbContext.Players.LongCount();
    }
    
    public IEnumerable<Player> GetPlayers(int page) => dbContext.Players.Skip(page * 50).Take(50);

    public bool IsOnline(Player player)
    {
        return sessionManager.IsSessionValid(new CSteamID(player.SteamId));
    }

    public IActionResult OnPostBan(ulong steamId)
    {
        var steamIdValue = new CSteamID(steamId);
        if (!steamIdValue.IsValid() || steamIdValue.IsLobby())
        {
            return RedirectToPage(new { message = "invalid steam id" });
        }
        
        if (!lobbyManager.IsInLobby())
        {
            return RedirectToPage(new { message = "server is not in lobby" });
        }
        
        sessionManager.BanPlayer(steamIdValue);
        return RedirectToPage(new { message = $"player {steamId} banned." });
    }
}