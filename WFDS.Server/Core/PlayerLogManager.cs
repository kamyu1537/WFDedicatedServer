using System.Text.Json;
using Steamworks;
using WFDS.Common.Actor;
using WFDS.Common.Network;
using WFDS.Database;
using WFDS.Database.DbSet;

namespace WFDS.Server.Core;

public class PlayerLogManager(DatabaseContext dbContext, IActorManager actorManager, ILogger<PlayerLogManager> logger)
{
    public void Append(Session session, string action, string message, object? data)
    {
        var actor = actorManager.GetPlayerActor(session.SteamId);
        if (actor == null)
        {
            logger.LogWarning("player actor not found");
            return;
        }

        var log = new PlayerLog
        {
            PlayerId = session.SteamId.m_SteamID,
            DisplayName = session.Name,
            Zone = actor.Zone,
            ZoneOwner = actor.ZoneOwner,
            PositionX = actor.Position.X,
            PositionY = actor.Position.Y,
            PositionZ = actor.Position.Z,
            Action = action,
            Message = message,
            JsonData = JsonSerializer.Serialize(data),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        dbContext.PlayerLogs.Add(log);
        dbContext.SaveChanges();
    }

    public void Append(CSteamID steamId, string displayName, string action, string message, object? data)
    {
        var log = new PlayerLog
        {
            PlayerId = steamId.m_SteamID,
            DisplayName = displayName,
            Zone = "",
            ZoneOwner = -1,
            PositionX = 0,
            PositionY = 0,
            PositionZ = 0,
            Action = action,
            Message = message,
            JsonData = JsonSerializer.Serialize(data),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        dbContext.PlayerLogs.Add(log);
        dbContext.SaveChanges();
    }
}