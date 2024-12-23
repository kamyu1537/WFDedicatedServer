﻿using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WFDS.Common.Actor;
using WFDS.Server.Core.Actor;

namespace WFDS.Server.Controllers;

[ApiController]
[Tags("event")]
[Route("api/v1/event")]
public sealed class EventController(IActorSpawnManager manager) : Controller
{
    [HttpPost("bird")]
    [SwaggerOperation("spawn ambient bird actor")]
    public IActionResult SpawnBird()
    {
        manager.SpawnAmbientBirdActor();
        return Ok();
    }
    
    [HttpPost("fish")]
    [SwaggerOperation("spawn fish spawn actor")]
    public IActionResult SpawnFish()
    {
        var actor = manager.SpawnFishSpawnActor();
        return actor != null ? Json(actor.ToDynamic()) : NotFound();
    }
    
    [HttpPost("alien")]
    [SwaggerOperation("spawn fish spawn alien actor")]
    public IActionResult SpawnAlien()
    {
        var actor = manager.SpawnFishSpawnAlienActor();
        return actor != null ? Json(actor.ToDynamic()) : NotFound();
    }
    
    [HttpPost("raincloud")]
    [SwaggerOperation("spawn rain cloud actor")]
    public IActionResult SpawnRaincloud()
    {
        var actor = manager.SpawnRainCloudActor();
        return actor != null ? Json(actor.ToDynamic()) : NotFound();
    }
    
    [HttpPost("portal")]
    [SwaggerOperation("spawn void portal actor")]
    public IActionResult SpawnPortal()
    {
        var actor = manager.SpawnVoidPortalActor();
        return actor != null ? Json(actor.ToDynamic()) : NotFound();
    }
    
    [HttpPost("metal")]
    [SwaggerOperation("spawn metal actor")]
    public IActionResult SpawnMetal()
    {
        var actor = manager.SpawnMetalActor();
        return actor != null ? Json(actor.ToDynamic()) : NotFound();
    }
}