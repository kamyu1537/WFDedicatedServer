using WFDS.Common.Types;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.PacketHandlers;

[PacketType("handshake")]
public class HandshakeHandler(ILogger<HandshakeHandler> logger) : PacketHandler<HandshakePacket>
{
    protected override async Task HandlePacketAsync(IGameSession sender, NetChannel channel, HandshakePacket packet)
    {
        logger.LogDebug("received handshake from {Sender} : {UserId}", sender.Friend, packet.UserId);

        sender.HandshakeReceived = true;
        sender.HandshakeReceiveTime = DateTimeOffset.UtcNow;
        await Task.Yield();
    }
}