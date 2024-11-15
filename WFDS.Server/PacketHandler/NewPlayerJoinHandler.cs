using WFDS.Common.ChannelEvents.Events;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Server.Core.ChannelEvent;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("new_player_join")]
internal class NewPlayerJoinHandler(ILogger<NewPlayerJoinHandler> logger) : PacketHandler<NewPlayerJoinPacket>
{
    protected override async Task HandlePacketAsync(Session sender, NetChannel channel, NewPlayerJoinPacket packet)
    {
        logger.LogDebug("received new_player_join from {Sender} on channel {Channel}", sender.SteamId, channel);
        await ChannelEventBus.PublishAsync(new NewPlayerJoinEvent(sender.SteamId));
    }
}