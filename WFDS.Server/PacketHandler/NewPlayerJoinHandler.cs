using WFDS.Common.GameEvents.Events;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Server.Core.GameEvent;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("new_player_join")]
public class NewPlayerJoinHandler(ILogger<NewPlayerJoinHandler> logger) : PacketHandler<NewPlayerJoinPacket>
{
    protected override void Handle(Session sender, NetChannel channel, NewPlayerJoinPacket packet)
    {
        logger.LogDebug("received new_player_join from {Sender} on channel {Channel}", sender.SteamId, channel);
        GameEventBus.Publish(new NewPlayerJoinEvent(sender.SteamId));
    }
}