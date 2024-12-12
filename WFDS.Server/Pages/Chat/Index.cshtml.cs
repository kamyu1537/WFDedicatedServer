using System.Numerics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using WFDS.Database;
using WFDS.Database.DbSet;

namespace WFDS.Server.Pages.Chat;

public class Index(DatabaseContext dbContext, LobbyManager lobbyManager, SessionManager sessionManager) : PageModel
{
    public void OnGet()
    {
    }

    public IActionResult OnPostSend(string message)
    {
        if (!lobbyManager.IsInLobby())
        {
            return RedirectToPage(new { message = "server is not in lobby" });
        }

        var lobbyId = lobbyManager.GetLobbyId();

        var packet = new MessagePacket
        {
            Local = false, Position = Vector3.Zero,
            Color = "ffffff",
            Message = $"[SERVER : {message}]",
            Zone = string.Empty,
            ZoneOwner = -1
        };

        sessionManager.BroadcastP2PPacket(lobbyId, NetChannel.GameState, packet);
        var messageHistory = new ChatHistory
        {
            PlayerId = 0,
            DisplayName = "SERVER",
            Message = packet.Message,
            CreatedAt = DateTimeOffset.Now,
            IsLocal = false,
            PositionX = 0,
            PositionY = 0,
            PositionZ = 0,
            Zone = string.Empty,
            ZoneOwner = -1,
        };
        dbContext.ChatHistories.Add(messageHistory);
        dbContext.SaveChanges();

        return RedirectToPage();
    }

    public IEnumerable<ChatHistory> ChatHistories()
    {
        return dbContext.ChatHistories.OrderByDescending(x => x.Id).Take(100);
    }
}