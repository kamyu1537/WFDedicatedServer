using Microsoft.AspNetCore.Mvc.RazorPages;
using WFDS.Common.Steam;

namespace WFDS.Server.Pages.Session;

public class Banned(SessionManager sessionManager) : PageModel
{
    public IEnumerable<string> GetBannedPlayers() => sessionManager.GetBannedPlayers();
    
    public void OnGet()
    {
        
    }
}