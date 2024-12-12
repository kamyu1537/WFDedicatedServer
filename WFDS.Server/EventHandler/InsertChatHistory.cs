using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Database;
using WFDS.Database.DbSet;

namespace WFDS.Server.EventHandler;

public sealed class InsertChatHistory(DatabaseContext dbContext) : GameEventHandler<PlayerMessageEvent>
{
    protected override void Handle(PlayerMessageEvent e)
    {
        if (string.IsNullOrWhiteSpace(e.Message))
            return;
        
        var displayName = Steamworks.SteamFriends.GetFriendPersonaName(e.PlayerId) ?? "";
        var chatHistory = new ChatHistory
        {
            PlayerId = e.PlayerId.m_SteamID,
            DisplayName = displayName,
            Message = e.Message,
            IsLocal = e.Local,
            Zone = e.Zone,
            ZoneOwner = e.ZoneOwner,
            PositionX = e.Position.X,
            PositionY = e.Position.Y,
            PositionZ = e.Position.Z,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        dbContext.ChatHistories.Add(chatHistory);
        dbContext.SaveChanges();
    }
}