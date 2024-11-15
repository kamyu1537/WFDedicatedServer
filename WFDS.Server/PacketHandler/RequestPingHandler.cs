using Steamworks;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("request_ping")]
internal class RequestPingHandler(ISessionManager sessionManager) : PacketHandler<RequestPingPacket>
{
    protected override async Task HandlePacketAsync(Session sender, NetChannel channel, RequestPingPacket packet)
    {
        sender.PingReceiveTime = DateTimeOffset.UtcNow;
        
        sessionManager.SendP2PPacket(sender.SteamId, NetChannel.GameState, new SendPingPacket
        {
            FromId = SteamClient.SteamId
        });
        await Task.Yield();
    }
}