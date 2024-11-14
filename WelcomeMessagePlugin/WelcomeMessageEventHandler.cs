using System.Drawing;
using WFDS.Common.ChannelEvents;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Network.Packets;

namespace WelcomeMessagePlugin;

public class WelcomeMessageEventHandler(ISessionManager sessionManager) : ChannelEventHandler<PlayerJoinedEvent>
{
    protected override async Task HandleAsync(PlayerJoinedEvent e)
    {
        var player = sessionManager.GetSession(e.PlayerId);
        if (player is null) return;

        var packet = MessagePacket.Create("Welcome to the server!", Color.White);
        sessionManager.SendP2PPacket(player.SteamId, NetChannel.GameState, packet);
        await Task.Yield();
    }
}