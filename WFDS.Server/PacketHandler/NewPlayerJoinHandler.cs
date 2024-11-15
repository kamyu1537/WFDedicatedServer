using Steamworks;
using WFDS.Common.Actor;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("new_player_join")]
internal class NewPlayerJoinHandler(ILogger<NewPlayerJoinHandler> logger, IActorManager actorManager, ISessionManager sessionManager) : PacketHandler<NewPlayerJoinPacket>
{
    protected override async Task HandlePacketAsync(Session sender, NetChannel channel, NewPlayerJoinPacket packet)
    {
        logger.LogDebug("received new_player_join from {Sender} on channel {Channel}", sender.SteamId, channel);
        
        // send request_actors
        sessionManager.SendP2PPacket(sender.SteamId, NetChannel.GameState, new RequestActorsPacket {
            UserId = SteamClient.SteamId.Value.ToString()
        });
        
        // send all owned actors
        foreach (var instancePacket in actorManager.GetOwnedActors().Select(InstanceActorPacket.Create))
        {
            sessionManager.SendP2PPacket(sender.SteamId, NetChannel.GameState, instancePacket);   
        }
        
        await Task.Yield();
    }
}