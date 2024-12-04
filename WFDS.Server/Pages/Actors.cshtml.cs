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
}