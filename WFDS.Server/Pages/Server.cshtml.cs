using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WFDS.Server.Pages;

public class Server(IHostApplicationLifetime lifetime) : PageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostShutdownAsync()
    {
        lifetime.StopApplication();
        return RedirectToPage();
    }
}