using Steamworks;
using WFDS.Common.Types;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("new_player_join")]
public class NewPlayerJoinHandler : PacketHandler<NewPlayerJoinPacket>
{
    protected override void HandlePacket(ISession sender, NetChannel channel, NewPlayerJoinPacket packet)
    {
        Logger.LogInformation("received new_player_join from {Sender} on channel {Channel}", sender.SteamId, channel);
        
        // request actors
        sender.SendP2PPacket(NetChannel.GameState, new RequestActorsPacket
        {
            UserId = SteamClient.SteamId.Value.ToString()
        });
    }
}