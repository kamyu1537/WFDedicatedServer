using Steamworks;
using WFDS.Common.Network;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Common.Network.Packets;
using ISession = WFDS.Common.Types.ISession;

namespace WFDS.Server.PacketHandlers;

[PacketType("request_ping")]
public class RequestPingHandler(ISessionManager sessionManager) : PacketHandler<RequestPingPacket>
{
    protected override async Task HandlePacketAsync(ISession sender, NetChannel channel, RequestPingPacket packet)
    {
        sender.PingReceiveTime = DateTimeOffset.UtcNow;
        
        sessionManager.SendP2PPacket(sender.SteamId, NetChannel.GameState, new SendPingPacket
        {
            FromId = SteamClient.SteamId
        });
        await Task.Yield();
    }
}