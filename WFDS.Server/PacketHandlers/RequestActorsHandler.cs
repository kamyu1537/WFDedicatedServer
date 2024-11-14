using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Network;
using WFDS.Network.Packets;

namespace WFDS.Server.PacketHandlers;

[PacketType("request_actors")]
public class RequestActorsHandler(ILogger<RequestActorsHandler> logger, IActorManager actorManager, ISessionManager sessionManager) : PacketHandler<RequestActorsPacket>
{
    protected override async Task HandlePacketAsync(IGameSession sender, NetChannel channel, RequestActorsPacket _)
    {
        logger.LogDebug("received request_actors from {Sender} on channel {Channel}", sender.SteamId, channel);

        var packet = new ActorRequestSendPacket();
        var owned = actorManager.GetOwnedActors().Select(ActorReplicationData.FromActor);
        packet.Actors.AddRange(owned);
        
        sessionManager.SendP2PPacket(sender.SteamId, NetChannel.GameState, packet);
        await Task.Yield();
    }
}