using System.Drawing;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace TemplatePlugin;

public class PlayerJoinGameEventHandler(ISessionManager sessionManager) : GameEventHandler<PlayerJoinedEvent>
{
    protected override void Handle(PlayerJoinedEvent e)
    {
        var player = sessionManager.GetSession(e.PlayerId);
        if (player is null) return;

        var packet = MessagePacket.Create("Welcome to the server!", Color.White);
        sessionManager.SendP2PPacket(player.SteamId, NetChannel.GameState, packet);
    }
}