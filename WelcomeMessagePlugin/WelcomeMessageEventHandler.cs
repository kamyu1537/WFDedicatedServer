using System.Drawing;
using WFDS.Common.ChannelEvents;
using WFDS.Common.Types.Manager;

namespace WelcomeMessagePlugin;

public class WelcomeMessageEventHandler(IGameSessionManager sessionManager) : ChannelEventHandler<PlayerJoinedEvent>
{
    protected override async Task HandleAsync(PlayerJoinedEvent e)
    {
        var player = sessionManager.GetSession(e.PlayerId);
        if (player is null) return;

        player.SendMessage("Welcome to the server!", Color.White);
        await Task.Yield();
    }
}