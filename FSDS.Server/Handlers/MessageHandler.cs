using FSDS.Server.Common;
using FSDS.Server.Packets;

namespace FSDS.Server.Handlers;

[PacketType("message")]
public class MessageHandler : PacketHandler
{
    public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = new MessagePacket();
        packet.Parse(data);

        Logger.LogInformation("received message from {Sender} on channel {Channel} / {Message}", sender.SteamId, channel, packet.Message);
    }
}