using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.Controllers;

[ApiController]
[Tags("session")]
[Route("api/v1/session")]

public class SessionController(IGameSessionManager manager) : Controller
{
    [HttpGet("info")]
    [SwaggerOperation("get lobby info")]
    public IActionResult GetLobbyInfo()
    {
        return Json(new
        {
            Code = manager.GetCode(),
            Name = manager.GetName(),
            LobbyType = manager.GetLobbyType(),
            Public = manager.IsPublic(),
            Adult = manager.IsAdult(),
            Capacity = manager.GetCapacity(),
            Current = manager.GetSessionCount(),
        });
    }
    
    [HttpGet("list")]
    [SwaggerOperation("get sessions")]
    public IActionResult GetSessions()
    {
        var sessions = manager.GetSessions();
        return Json(new
        {
            Count = sessions.Length,
            Sessions = sessions.Select(x => x.Friend.ToString()).ToImmutableArray()
        });
    }

    [HttpGet("{steamId}")]
    [SwaggerOperation("get session detail")]
    public IActionResult GetSession(ulong steamId)
    {
        var session = manager.GetSession(steamId);
        if (session == null)
        {
            return NotFound();
        }
        
        return Json(new
        {
            session.SteamId,
            session.Friend,
            session.ActorCreated,
            session.ConnectTime,
            session.HandshakeReceiveTime,
            session.PingReceiveTime,
            session.PacketReceiveTime,
            Actor = session.Actor == null ? null
                : new
                {
                    session.Actor.ActorId,
                    session.Actor.ActorType,
                    session.Actor.CreatorId,
                    session.Actor.Zone,
                    session.Actor.ZoneOwner,
                    session.Actor.HeldItem,
                    session.Actor.Cosmetics,
                    session.Actor.Position,
                    session.Actor.Rotation,
                    session.Actor.Decay,
                    session.Actor.DecayTimer,
                    session.Actor.CreateTime,
                    session.Actor.IsDeadActor,
                    session.Actor.NetworkShareDefaultCooldown,
                    session.Actor.NetworkShareCooldown,
                },
        });
    }
    
    [HttpPost("{steamId}/kick")]
    [SwaggerOperation("kick player")]
    public IActionResult KickPlayer(ulong steamId)
    {
        manager.KickPlayer(steamId);
        return Ok();
    }
    
    [HttpPost("{steamId}/temp_ban")]
    [SwaggerOperation("ban player")]
    public IActionResult TempBanPlayer(ulong steamId)
    {
        manager.TempBanPlayer(steamId);
        return Ok();
    }
    
    [HttpDelete("{steamId}/temp_ban")]
    [SwaggerOperation("unban player")]
    public IActionResult RemoveBanPlayer(ulong steamId)
    {
        manager.RemoveBanPlayer(steamId);
        return Ok();
    }
}