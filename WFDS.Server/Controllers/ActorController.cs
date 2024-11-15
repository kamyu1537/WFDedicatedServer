using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WFDS.Common.Actor;

namespace WFDS.Server.Controllers;

[ApiController]
[Tags("actor")]
[Route("api/v1/actor")]
public class ActorController(IActorManager manager) : Controller
{
    [HttpGet("list")]
    public IActionResult GetActors()
    {
        var actors = manager.GetActors().ToArray();
        
        return Json(new
        {
            Count = actors.Length,
            Actors = actors.Select(x => $"{x.Type} ({x.ActorId})").ToArray()
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
            Actors = actors.Select(x => $"{x.Type} ({x.ActorId})")
        });
    }
    
    [HttpGet("creator/{creatorId}")]
    [SwaggerOperation("get actors by creator id")]
    public IActionResult GetActorsByCreatorId(ulong creatorId)
    {
        var actors = manager.GetActorsByCreatorId(creatorId).ToArray();
        
        return Json(new
        {
            Count = actors.Length,
            Actors = actors.Select(x => $"{x.Type} ({x.ActorId})")
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
            Actors = actors.Select(x => $"{x.Type} ({x.ActorId})")
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
        
        return Json(new
        {
            actor.ActorId,
            actor.Type,
            actor.CreatorId,
            actor.Zone,
            actor.ZoneOwner,
            actor.Position,
            actor.Rotation,
            actor.Decay,
            actor.DecayTimer,
            actor.CreateTime,
            actor.IsDeadActor,
            actor.NetworkShareDefaultCooldown,
            actor.NetworkShareCooldown,
        });
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