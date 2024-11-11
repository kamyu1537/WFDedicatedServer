using Steamworks;
using WFDS.Common.Types;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("request_ping")]
public class RequestPingHandler : PacketHandler
{
    public override void HandlePacket(ISession sender, NetChannel channel, Dictionary<object, object> data)
    {
        sender.PingReceiveTime = DateTimeOffset.UtcNow;
        sender.SendP2PPacket(NetChannel.GameState, new SendPingPacket
        {
            FromId = SteamClient.SteamId
        });
    }
}