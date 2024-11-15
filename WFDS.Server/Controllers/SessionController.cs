using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.Controllers;

[ApiController]
[Tags("session")]
[Route("api/v1/session")]

internal class SessionController(ISessionManager manager) : Controller
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
        var sessions = manager.GetSessions().ToArray();
        return Json(new
        {
            Count = sessions.Length,
            Sessions = sessions.Select(x => x.Friend.ToString())
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
            session.ConnectTime,
            session.HandshakeReceiveTime,
            session.PingReceiveTime,
            session.PacketReceiveTime,
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