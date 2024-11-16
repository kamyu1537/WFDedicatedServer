using WFDS.Common.GameEvents.Events;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Server.Core.GameEvent;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("message")]
internal class MessageHandler(ILogger<MessageHandler> logger) : PacketHandler<MessagePacket>
{
    protected override async Task HandlePacketAsync(Session sender, NetChannel channel, MessagePacket packet)
    {
        logger.LogDebug("received message from {Sender} ({Zone}/{ZoneOwner}) on channel {Channel} / [{Color}] {Message}", sender.SteamId, packet.Zone, packet.ZoneOwner, channel, packet.Color, packet.Message);
        GameEventBus.Publish(new PlayerMessageEvent(sender.SteamId, packet.Message, packet.Color, packet.Local, packet.Position, packet.Zone, packet.ZoneOwner));
        await Task.CompletedTask;
    }
}