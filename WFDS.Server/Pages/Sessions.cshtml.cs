using Microsoft.AspNetCore.Mvc.RazorPages;
using WFDS.Common.Network;
using WFDS.Common.Steam;

namespace WFDS.Server.Pages;

public class Sessions(SessionManager sessionManager, LobbyManager lobbyManager) : PageModel
{
    public int MaxSessionCount { get; } = lobbyManager.GetCap();
    public int SessionCount { get; } = sessionManager.GetSessionCount();
    public IEnumerable<Session> GetSessions() => sessionManager.GetSessions();

    public void OnGet()
    {
        ViewData["Title"] = "Sessions";
    }
}