using WFDS.Common.Actor;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("request_actors")]
public sealed class RequestActorsHandler(ILogger<RequestActorsHandler> logger, IActorManager actorManager, SessionManager sessionManager) : PacketHandler<RequestActorsPacket>
{
    protected override void Handle(Session sender, NetChannel channel, RequestActorsPacket _)
    {
        logger.LogDebug("received request_actors from {Sender} on channel {Channel}", sender.SteamId, channel);

        var send = new ActorRequestSendPacket();
        var actors = actorManager.GetOwnedActors().Where(x => !x.IsRemoved).Select(ActorReplicationData.FromActor);
        send.Actors.AddRange(actors);
        sessionManager.SendPacket(sender.SteamId, NetChannel.GameState, send);
    }
}