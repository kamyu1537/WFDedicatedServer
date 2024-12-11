using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("handshake")]
public sealed class HandshakeHandler(ILogger<HandshakeHandler> logger) : PacketHandler<HandshakePacket>
{
    protected override void Handle(Session sender, NetChannel channel, HandshakePacket packet)
    {
        logger.LogInformation("received handshake from {Sender} : {UserId}", sender, packet.UserId);

        sender.HandshakeReceived = true;
        sender.HandshakeReceiveTime = DateTimeOffset.UtcNow;
    }
}