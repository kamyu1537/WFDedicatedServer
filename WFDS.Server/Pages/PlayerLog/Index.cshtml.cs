using System.Numerics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using WFDS.Database;

namespace WFDS.Server.Pages.PlayerLog;

public class Index(DatabaseContext dbContext, LobbyManager lobbyManager, SessionManager sessionManager) : PageModel
{
    public void OnGet()
    {
    }

    public IActionResult OnPostSendServerChat(string message)
    {
        if (!lobbyManager.IsInLobby())
        {
            return RedirectToPage(new { message = "server is not in lobby" });
        }

        var packet = new MessagePacket
        {
            Local = false, Position = Vector3.Zero,
            Color = "ffffff",
            Message = $"<SERVER: {message}>",
            Zone = string.Empty,
            ZoneOwner = -1
        };

        sessionManager.BroadcastPacket(NetChannel.GameState, packet);
        var messageHistory = new Database.DbSet.PlayerLog()
        {
            PlayerId = 0,
            DisplayName = "SERVER",
            Action = "message",
            Message = packet.Message,
            CreatedAt = DateTimeOffset.Now,
            JsonData = JsonSerializer.Serialize(new { is_local = false, color = packet.Color }),
            PositionX = 0,
            PositionY = 0,
            PositionZ = 0,
            Zone = string.Empty,
            ZoneOwner = -1,
        };
        dbContext.PlayerLogs.Add(messageHistory);
        dbContext.SaveChanges();

        return RedirectToPage();
    }

    public IEnumerable<Database.DbSet.PlayerLog> GetLogs(int page = 1)
    {
        return dbContext.PlayerLogs.OrderByDescending(x => x.Id).Skip((page - 1) * 100).Take(100);
    }

    public long GetTotalLogCount()
    {
        return dbContext.PlayerLogs.LongCount();
    }
}