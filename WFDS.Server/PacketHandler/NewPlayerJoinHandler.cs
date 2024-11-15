using Steamworks;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("new_player_join")]
internal class NewPlayerJoinHandler(ILogger<NewPlayerJoinHandler> logger, ISessionManager sessionManager) : PacketHandler<NewPlayerJoinPacket>
{
    protected override async Task HandlePacketAsync(Session sender, NetChannel channel, NewPlayerJoinPacket packet)
    {
        logger.LogDebug("received new_player_join from {Sender} on channel {Channel}", sender.SteamId, channel);
        
        // request actors
        sessionManager.BroadcastP2PPacket(NetChannel.GameState, new RequestActorsPacket
        {
            UserId = SteamClient.SteamId.Value.ToString()
        });
        await Task.Yield();
    }
}