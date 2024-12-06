using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WFDS.Common.Actor;

namespace WFDS.Server.Pages.Actor;

public class Index(IActorManager actorManager) : PageModel
{
    public int GetOwnedActorCount() => actorManager.GetOwnedActorCount();
    public IEnumerable<IActor> GetOwnedActors() => actorManager.GetOwnedActors();

    public void OnGet()
    {
    }

    public IActionResult OnPostDelete(long id)
    {
        string message;
        if (actorManager.TryRemoveActor(id, ActorRemoveTypes.QueueFree, out _))
        {
            message = $"actor {id} deleted";   
        }
        else
        {
            message = $"actor {id} deleted failed";
        }
        
        return RedirectToPage(new { message });
    }
}