﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Steamworks;
using WFDS.Common.Steam;
using WFDS.Database;
using WFDS.Database.DbSet;

namespace WFDS.Server.Pages.Session;

public class Banned(DatabaseContext dbContext, SessionManager sessionManager, LobbyManager lobbyManager) : PageModel
{
    public int GetBannedPlayerCount() => dbContext.BannedPlayers.Count();
    public IEnumerable<BannedPlayer> GetBannedPlayers() => dbContext.BannedPlayers;
    
    public void OnGet()
    {
        
    }
    
    public IActionResult OnPostUnban(ulong steamId)
    {
        var steamIdValue = new CSteamID(steamId);
        if (!steamIdValue.IsValid() && !steamIdValue.IsLobby())
        {
            return RedirectToPage(new { message = "invalid steam id" });
        }

        if (!lobbyManager.IsInLobby())
        {
            return RedirectToPage(new { message = "server is not in lobby" });
        }
        
        sessionManager.RemoveBanPlayer(steamIdValue);
        return RedirectToPage(new { message = $"player {steamId} unbanned."});
    }
}