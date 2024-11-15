using WFDS.Common.ChannelEvents;
using WFDS.Common.ChannelEvents.Events;
using WFDS.Common.Network;
using WFDS.Common.Types;
using WFDS.Common.Network.Packets;
using ISession = WFDS.Common.Types.ISession;

namespace WFDS.Server.PacketHandlers;

[PacketType("message")]
public class MessageHandler(ILogger<MessageHandler> logger) : PacketHandler<MessagePacket>
{
    protected override async Task HandlePacketAsync(ISession sender, NetChannel channel, MessagePacket packet)
    {
        logger.LogDebug("received message from {Sender} ({Zone}/{ZoneOwner}) on channel {Channel} / [{Color}] {Message}", sender.SteamId, packet.Zone, packet.ZoneOwner, channel, packet.Color, packet.Message);
        await ChannelEventBus.PublishAsync(new PlayerMessageEvent(sender.SteamId, packet.Message, packet.Color, packet.Local, packet.Position, packet.Zone, packet.ZoneOwner));
    }
}