using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WFDS.Common.Actor;
using WFDS.Common.Extensions;
using WFDS.Server.Core.Actor;

namespace WFDS.Server.Controllers;

[ApiController]
[Tags("actor")]
[Route("api/v1/actor")]
public sealed class ActorController(IActorManager manager) : Controller
{
    [HttpGet("list")]
    public IActionResult GetActors()
    {
        var actors = manager.GetActors().ToArray();
        
        return Json(new
        {
            Count = actors.Length,
            Actors = actors.Select(x => new { x.ActorId, Type = x.Type.ToString() }).ToArray()
        });
    }
    
    [HttpGet("type/{type}")]
    [SwaggerOperation("get actors by type")]
    public IActionResult GetActorsByType(string type)
    {
        var actorType = ActorType.GetActorType(type);
        if (actorType == null)
        {
            return BadRequest();
        }
        
        var actors = manager.GetActorsByType(actorType).ToArray();
        
        return Json(new
        {
            Count = actors.Length,
            Actors = actors.Select(x => new { x.ActorId, Type = x.Type.ToString() })
        });
    }
    
    [HttpGet("creator/{creatorId}")]
    [SwaggerOperation("get actors by creator id")]
    public IActionResult GetActorsByCreatorId(string creatorId)
    {
        if (!ulong.TryParse(creatorId, NumberStyles.Any, CultureInfo.InvariantCulture, out var id))
        {
            return BadRequest();
        }
        
        var actors = manager.GetActorsByCreatorId(id.ToSteamID()).ToArray();
        
        return Json(new
        {
            Count = actors.Length,
            Actors = actors.Select(x => new { x.ActorId, Type = x.Type.ToString() })
        });
    }
    
    [HttpGet("owned")]
    [SwaggerOperation("get owned actors")]
    public IActionResult GetOwnedActors()
    {
        var actors = manager.GetOwnedActors().ToArray();
        
        return Json(new
        {
            Count = actors.Length,
            Actors = actors.Select(x => new { x.ActorId, Type = x.Type.ToString() })
        });
    }
    
    [HttpGet("{actorId}")]
    [SwaggerOperation("get actor detail")]
    public IActionResult GetActor(long actorId)
    {
        var actor = manager.GetActor(actorId);
        if (actor == null)
        {
            return NotFound();
        }
        
        return Json(actor.ToDynamic());
    }
    
    [HttpDelete("{actorId}")]
    [SwaggerOperation("remove owned actor")]
    public IActionResult RemoveActor(long actorId)
    {
        if (!manager.TryRemoveActor(actorId, ActorRemoveTypes.None, out _))
        {
            return NotFound();
        }
        return Ok();
    }
}