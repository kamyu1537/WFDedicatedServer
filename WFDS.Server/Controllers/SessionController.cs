using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WFDS.Common.Extensions;
using WFDS.Common.Steam;

namespace WFDS.Server.Controllers;

[ApiController]
[Tags("session")]
[Route("api/v1/session")]

public class SessionController(SessionManager manager, LobbyManager lobby) : Controller
{
    [HttpGet("info")]
    [SwaggerOperation("get lobby info")]
    public IActionResult GetLobbyInfo()
    {
        return Json(new
        {
            Code = lobby.GetCode(),
            Name = lobby.GetName(),
            LobbyType = lobby.GetLobbyType(),
            Adult = lobby.IsAdult(),
            Capacity = lobby.GetCap(),
            Current = manager.GetSessionCount(),
            BannedPlayers = manager.GetBannedPlayers()
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
            Sessions = sessions.Select(x => new
            {
                x.Name,
                SteamId = x.SteamId.m_SteamID.ToString(CultureInfo.InvariantCulture)
            })
        });
    }

    [HttpGet("{steamId}")]
    [SwaggerOperation("get session detail")]
    public IActionResult GetSession(ulong steamId)
    {
        var session = manager.GetSession(steamId.ToSteamID());
        if (session == null)
        {
            return NotFound();
        }
        
        return Json(new
        {
            SteamId = session.SteamId.m_SteamID.ToString(CultureInfo.InvariantCulture),
            session.Name,
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
        manager.KickPlayer(steamId.ToSteamID());
        return Ok();
    }
    
    [HttpPost("{steamId}/temp_ban")]
    [SwaggerOperation("ban player")]
    public IActionResult TempBanPlayer(ulong steamId)
    {
        manager.BanPlayer(lobby.GetLobbyId(), steamId.ToSteamID());
        return Ok();
    }
    
    [HttpDelete("{steamId}/temp_ban")]
    [SwaggerOperation("unban player")]
    public IActionResult RemoveBanPlayer(ulong steamId)
    {
        manager.RemoveBanPlayer(steamId.ToSteamID());
        return Ok();
    }
}