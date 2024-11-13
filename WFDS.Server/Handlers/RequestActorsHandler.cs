using WFDS.Common.Types;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("request_actors")]
public class RequestActorsHandler : PacketHandler<RequestActorsPacket>
{
    protected override void HandlePacket(IGameSession sender, NetChannel channel, RequestActorsPacket _)
    {
        Logger.LogInformation("received request_actors from {Sender} on channel {Channel}", sender.SteamId, channel);

        var packet = new ActorRequestSendPacket();
        ActorManager?.SelectOwnedActors(actor => packet.Actors.Add(ActorReplicationData.FromActor(actor)));
        sender.SendP2PPacket(NetChannel.GameState, packet);
    }
}