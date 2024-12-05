using Microsoft.AspNetCore.Mvc.RazorPages;
using WFDS.Common.Steam;

namespace WFDS.Server.Pages;

public class Index(LobbyManager lobbyManager) : PageModel
{
    public LobbyManager LobbyManager => lobbyManager;
    
    public void OnGet()
    {
    }
}