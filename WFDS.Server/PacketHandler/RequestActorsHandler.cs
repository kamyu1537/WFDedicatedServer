﻿using WFDS.Common.Actor;
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

        var packet = new ActorRequestSendPacket();
        var owned = actorManager.GetOwnedActors().Select(ActorReplicationData.FromActor);
        packet.Actors.AddRange(owned);
        sessionManager.SendP2PPacket(sender.SteamId, NetChannel.GameState, packet);
        await Task.Yield();
    }
}