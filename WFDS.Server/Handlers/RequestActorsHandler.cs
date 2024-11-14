using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("request_actors")]
public class RequestActorsHandler(ILogger<RequestActorsHandler> logger, IActorManager actorManager) : PacketHandler<RequestActorsPacket>
{
    protected override async Task HandlePacketAsync(IGameSession sender, NetChannel channel, RequestActorsPacket _)
    {
        logger.LogDebug("received request_actors from {Sender} on channel {Channel}", sender.SteamId, channel);

        var packet = new ActorRequestSendPacket();
        var owned = actorManager.GetOwnedActors().Select(ActorReplicationData.FromActor);
        packet.Actors.AddRange(owned);
        sender.SendP2PPacket(NetChannel.GameState, packet);
        await Task.Yield();
    }
}