using WFDS.Server.Common;
using WFDS.Server.Packets;
using Steamworks;

namespace WFDS.Server.Handlers;

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