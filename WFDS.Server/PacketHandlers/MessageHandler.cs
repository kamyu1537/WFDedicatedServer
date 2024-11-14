using WFDS.Common.ChannelEvents;
using WFDS.Common.Types;
using WFDS.Network;
using WFDS.Network.Packets;

namespace WFDS.Server.PacketHandlers;

[PacketType("message")]
public class MessageHandler(ILogger<MessageHandler> logger) : PacketHandler<MessagePacket>
{
    protected override async Task HandlePacketAsync(IGameSession sender, NetChannel channel, MessagePacket packet)
    {
        logger.LogDebug("received message from {Sender} ({Zone}/{ZoneOwner}) on channel {Channel} / [{Color}] {Message}", sender.SteamId, packet.Zone, packet.ZoneOwner, channel, packet.Color, packet.Message);
        await ChannelEvent.PublishAsync(new PlayerMessageEvent(sender.SteamId, packet.Message, packet.Color, packet.Local, packet.Position, packet.Zone, packet.ZoneOwner));
    }
}