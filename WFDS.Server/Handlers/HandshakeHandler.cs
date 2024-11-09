using WFDS.Server.Common;
using WFDS.Server.Packets;
using Steamworks;

namespace WFDS.Server.Handlers;

[PacketType("handshake")]
public class HandshakeHandler : PacketHandler
{
    public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
    {
        Logger.LogInformation("received handshake from {Sender} on channel {Channel}", sender.SteamId, channel);

        sender.HandshakeReceived = true;
        sender.HandshakeReceiveTime = DateTimeOffset.UtcNow;
    }
}