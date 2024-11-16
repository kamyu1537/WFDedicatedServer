using WFDS.Common.Actor;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("request_actors")]
internal class RequestActorsHandler(ILogger<RequestActorsHandler> logger, IActorManager actorManager, ISessionManager sessionManager) : PacketHandler<RequestActorsPacket>
{
    protected override async Task HandlePacketAsync(Session sender, NetChannel channel, RequestActorsPacket _)
    {
        logger.LogDebug("received request_actors from {Sender} on channel {Channel}", sender.SteamId, channel);
        sessionManager.SendP2PPacket(sender.SteamId, NetChannel.GameState, new ActorRequestSendPacket());

        // actor data update
        var owned = actorManager.GetOwnedActors().Where(x => !x.IsRemoved).Where(x => !x.IsDead);
        foreach (var actor in owned)
        {
            sessionManager.SendP2PPacket(sender.SteamId, NetChannel.GameState, InstanceActorPacket.Create(actor));
        }

        await Task.Yield();
    }
}