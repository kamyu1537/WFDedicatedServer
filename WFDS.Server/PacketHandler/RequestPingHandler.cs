using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using WFDS.Server.Core.Network;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("request_ping")]
public class RequestPingHandler(SessionManager sessionManager, SteamManager steam) : PacketHandler<RequestPingPacket>
{
    protected override void Handle(Session sender, NetChannel channel, RequestPingPacket packet)
    {
        sender.PingReceiveTime = DateTimeOffset.UtcNow;

        sessionManager.SendP2PPacket(sender.SteamId, NetChannel.GameState, new SendPingPacket
        {
            FromId = steam.SteamId
        });
    }
}