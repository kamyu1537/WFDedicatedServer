using WFDS.Server.Common;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("message")]
public class MessageHandler : PacketHandler
{
    public override void HandlePacket(ISession sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = new MessagePacket();
        packet.Parse(data);

        Logger.LogInformation("received message from {Sender} ({Zone}/{ZoneOwner}) on channel {Channel} / [{Color}] {Message}", sender.SteamId, packet.Zone, packet.ZoneOwner, channel, packet.Color, packet.Message);
    }
}