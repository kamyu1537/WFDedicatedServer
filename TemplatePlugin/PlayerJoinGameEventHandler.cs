using System.Drawing;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;

namespace TemplatePlugin;

public class PlayerJoinGameEventHandler(SessionManager sessionManager) : GameEventHandler<PlayerJoinedEvent>
{
    protected override void Handle(PlayerJoinedEvent e)
    {
        var player = sessionManager.GetSession(e.PlayerId);
        if (player is null) return;

        var packet = MessagePacket.Create("Welcome to the server!", Color.White);
        sessionManager.SendPacket(player.SteamId, NetChannel.GameState, packet);
    }
}