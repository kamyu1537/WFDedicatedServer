using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Server.Core.Actor;
using WFDS.Server.Core.Network;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("new_player_join")]
public class NewPlayerJoinHandler(ILogger<NewPlayerJoinHandler> logger, CanvasManager canvas, SessionManager session) : PacketHandler<NewPlayerJoinPacket>
{
    protected override void Handle(Session sender, NetChannel channel, NewPlayerJoinPacket packet)
    {
        logger.LogDebug("received new_player_join from {Sender} on channel {Channel}", sender.SteamId, channel);
        GameEventBus.Publish(new NewPlayerJoinEvent(sender.SteamId));

        var chalks = canvas.GetCanvasPackets();
        foreach (var chalk in chalks)
        {
            session.SendP2PPacket(sender.SteamId, NetChannel.Chalk, chalk);
        }
    }
}