using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WFDS.Common.Types;

namespace WFDS.Server.Pages.Canvas;

public class Index(ICanvasManager canvasManager) : PageModel
{
    public Dictionary<string, object>[] CanvasData => canvasManager.GetCanvasPackets().Select(p => new Dictionary<string, object>()
    {
        { "canvas_id", p.CanvasId },
        {
            "data", p.GetData().Select(x => new Dictionary<string, object>()
            {
                { "pos", new Dictionary<string, float> { { "x", x.pos.X }, { "y", x.pos.Y } } },
                { "color", x.color }
            }).ToArray()
        }
    }).ToArray();

    public void OnGet()
    {
    }

    public IActionResult OnPostClear()
    {
        canvasManager.ClearAll();
        return RedirectToPage();
    }
}