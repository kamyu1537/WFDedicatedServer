using WFDS.Common.ActorEvents;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("message")]
public class MessageHandler(ILogger<MessageHandler> logger) : PacketHandler<MessagePacket>
{
    protected override async Task HandlePacketAsync(IGameSession sender, NetChannel channel, MessagePacket packet)
    {
        logger.LogDebug("received message from {Sender} ({Zone}/{ZoneOwner}) on channel {Channel} / [{Color}] {Message}", sender.SteamId, packet.Zone, packet.ZoneOwner, channel, packet.Color, packet.Message);

        if (sender.Actor == null)
        {
            logger.LogWarning("received message from {Sender} without an actor", sender.SteamId);
            return;
        }
        
        await ActorEventChannel.PublishAsync(new PlayerMessageEvent(sender.Actor.ActorId, packet.Message, packet.Color, packet.Local, packet.Position, packet.Zone, packet.ZoneOwner));
    }
}