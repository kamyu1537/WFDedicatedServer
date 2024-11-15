using WFDS.Common.Network;
using WFDS.Common.Types;
using WFDS.Common.Network.Packets;
using ISession = WFDS.Common.Types.ISession;

namespace WFDS.Server.PacketHandlers;

[PacketType("handshake")]
public class HandshakeHandler(ILogger<HandshakeHandler> logger) : PacketHandler<HandshakePacket>
{
    protected override async Task HandlePacketAsync(ISession sender, NetChannel channel, HandshakePacket packet)
    {
        logger.LogDebug("received handshake from {Sender} : {UserId}", sender.Friend, packet.UserId);

        sender.HandshakeReceived = true;
        sender.HandshakeReceiveTime = DateTimeOffset.UtcNow;
        await Task.Yield();
    }
}