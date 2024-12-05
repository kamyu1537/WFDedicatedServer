using Microsoft.AspNetCore.Mvc.RazorPages;
using WFDS.Common.Actor;

namespace WFDS.Server.Pages.Actors;

public class All(IActorManager actorManager) : PageModel
{
    public int GetAllActorCount() => actorManager.GetActorCount();
    public IEnumerable<IActor> GetAllActors() => actorManager.GetActors();
    
    public void OnGet()
    {
        
    }
}