using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WFDS.Server.Pages.Server;

public class Index(IHostApplicationLifetime lifetime) : PageModel
{
    public void OnGet()
    {
    }

    public IActionResult OnPostShutdown()
    {
        lifetime.StopApplication();
        return RedirectToPage();
    }
}