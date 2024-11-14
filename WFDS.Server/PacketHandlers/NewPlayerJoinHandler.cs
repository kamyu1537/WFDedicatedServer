using Steamworks;
using WFDS.Common.Types;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.PacketHandlers;

[PacketType("new_player_join")]
public class NewPlayerJoinHandler(ILogger<NewPlayerJoinHandler> logger) : PacketHandler<NewPlayerJoinPacket>
{
    protected override async Task HandlePacketAsync(IGameSession sender, NetChannel channel, NewPlayerJoinPacket packet)
    {
        logger.LogDebug("received new_player_join from {Sender} on channel {Channel}", sender.SteamId, channel);
        
        // request actors
        sender.SendP2PPacket(NetChannel.GameState, new RequestActorsPacket
        {
            UserId = SteamClient.SteamId.Value.ToString()
        });
        await Task.Yield();
    }
}