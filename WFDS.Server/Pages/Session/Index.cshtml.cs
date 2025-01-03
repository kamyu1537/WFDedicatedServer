using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Steamworks;
using WFDS.Common.Steam;
using WFDS.Database;
using WFDS.Database.DbSet;

namespace WFDS.Server.Pages.Session;

public class Index(SessionManager sessionManager, LobbyManager lobbyManager) : PageModel
{
    public int MaxSessionCount { get; } = lobbyManager.GetCap();
    public int SessionCount { get; } = sessionManager.GetSessionCount();
    public IEnumerable<Common.Network.Session> GetSessions() => sessionManager.GetSessions();
    

    public void OnGet()
    {
        ViewData["Title"] = "Sessions";
    }

    public IActionResult OnPostKick(string steamId)
    {
        if (!ulong.TryParse(steamId, out var value))
        {
            return RedirectToPage();
        }
        
        var steamIdValue = new CSteamID(value);
        if (!steamIdValue.IsValid() || steamIdValue.IsLobby())
        {
            return RedirectToPage(new { message = "invalid steam id" });
        }

        sessionManager.KickPlayer(steamIdValue);
        return RedirectToPage(new { message = $"kicked {steamId}" });
    }
    
    public IActionResult OnPostBan(string steamId)
    {
        if (!ulong.TryParse(steamId, out var value))
        {
            return RedirectToPage();
        }
        
        var steamIdValue = new CSteamID(value);
        if (!steamIdValue.IsValid() || steamIdValue.IsLobby())
        {
            return RedirectToPage(new { message = "invalid steam id" });
        }

        sessionManager.BanPlayer(steamIdValue);
        return RedirectToPage(new { message = $"banned {steamId}" });
    }
}