using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WFDS.Common.Actor;

namespace WFDS.Server.Pages;

public class Events(IActorSpawnManager spawnManager) : PageModel
{
    public readonly Dictionary<string, Action> SpawnEvents = new()
    {
        { "ambient_bird", spawnManager.SpawnAmbientBirdActor },
        { "fish_spawn", () => spawnManager.SpawnFishSpawnActor() },
        { "fish_spawn_alien", () => spawnManager.SpawnFishSpawnAlienActor() },
        { "raincloud", () => spawnManager.SpawnRainCloudActor() },
        { "void_portal", () => spawnManager.SpawnVoidPortalActor() },
        { "metal_spawn", () => spawnManager.SpawnMetalActor() },
    };

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostSpawnAsync(string eventName)
    {
        if (!SpawnEvents.TryGetValue(eventName, out var action))
            return RedirectToPage(new { message = "event not found!" });

        action();
        return RedirectToPage(new { message = $"event {eventName} started" });
    }
}