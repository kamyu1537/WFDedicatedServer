using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("request_ping")]
public sealed class RequestPingHandler(SessionManager sessionManager, SteamManager steam) : PacketHandler<RequestPingPacket>
{
    protected override void Handle(Session sender, NetChannel channel, RequestPingPacket packet)
    {
        sender.PingReceiveTime = DateTimeOffset.UtcNow;

        sessionManager.SendPacket(sender.SteamId, NetChannel.GameState, new SendPingPacket
        {
            FromId = steam.SteamId
        });
    }
}