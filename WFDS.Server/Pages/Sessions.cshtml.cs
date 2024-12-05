using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Steamworks;
using WFDS.Common.Network;
using WFDS.Common.Steam;

namespace WFDS.Server.Pages;

public class Sessions(SessionManager sessionManager, LobbyManager lobbyManager) : PageModel
{
    public int MaxSessionCount { get; } = lobbyManager.GetCap();
    public int SessionCount { get; } = sessionManager.GetSessionCount();
    public IEnumerable<Session> GetSessions() => sessionManager.GetSessions();
    public IEnumerable<string> GetBannedPlayers() => sessionManager.GetBannedPlayers();

    public void OnGet()
    {
        ViewData["Title"] = "Sessions";
    }

    public async Task<IActionResult> OnPostKickAsync(string steamId)
    {
        if (!ulong.TryParse(steamId, out var value))
        {
            return RedirectToPage();
        }

        sessionManager.KickPlayer(new CSteamID(value));
        return RedirectToPage(new { message = $"kicked {steamId}" });
    }
    
    public async Task<IActionResult> OnPostBanAsync(string steamId)
    {
        if (!ulong.TryParse(steamId, out var value))
        {
            return RedirectToPage();
        }

        sessionManager.TempBanPlayer(lobbyManager.GetLobbyId(), new CSteamID(value));
        return RedirectToPage(new { message = $"banned {steamId}" });
    }
}