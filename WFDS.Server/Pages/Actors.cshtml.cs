using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WFDS.Common.Actor;

namespace WFDS.Server.Pages;

public class Actors(IActorManager actorManager) : PageModel
{
    public int GetOwnedActorCount() => actorManager.GetOwnedActorCount();
    public IEnumerable<IActor> GetOwnedActors() => actorManager.GetOwnedActors();

    public int GetAllActorCount() => actorManager.GetActorCount();
    public IEnumerable<IActor> GetAllActors() => actorManager.GetActors();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostDeleteAsync(long id)
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