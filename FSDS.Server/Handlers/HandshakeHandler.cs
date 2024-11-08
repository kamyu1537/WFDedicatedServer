using FSDS.Server.Common;
using FSDS.Server.Packets;
using Steamworks;

namespace FSDS.Server.Handlers;

[PacketType("handshake")]
public class HandshakeHandler : PacketHandler
{
    public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
    {
        Logger.LogInformation("received handshake from {Sender} on channel {Channel}", sender.SteamId, channel);

        sender.HandshakeReceived = true;
        sender.HandshakeReceiveTime = DateTimeOffset.UtcNow;

        sender.Send(NetChannel.GameState, new HandshakePacket
        {
            UserId = SteamClient.SteamId.Value.ToString()
        });
    }
}