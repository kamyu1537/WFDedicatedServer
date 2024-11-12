using WFDS.Common.Helpers;
using WFDS.Common.Types;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("handshake")]
public class HandshakeHandler : PacketHandler
{
    public override void HandlePacket(ISession sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = PacketHelper.FromDictionary<HandshakePacket>(data);
        
        Logger.LogInformation("received handshake from {Sender} : {UserId}", sender.Friend, packet.UserId);

        sender.HandshakeReceived = true;
        sender.HandshakeReceiveTime = DateTimeOffset.UtcNow;
    }
}