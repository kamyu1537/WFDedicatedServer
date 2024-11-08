using FSDS.Server.Common;
using FSDS.Server.Packets;
using Steamworks;

namespace FSDS.Server.Handlers;

[PacketType("request_ping")]
public class RequestPingHandler : PacketHandler
{
    public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
    {
        sender.PingReceiveTime = DateTimeOffset.UtcNow;
        sender.Send(NetChannel.GameState, new SendPingPacket
        {
            FromId = SteamClient.SteamId
        });
    }
}