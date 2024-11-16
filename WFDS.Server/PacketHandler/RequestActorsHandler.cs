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

        var owned = actorManager.GetOwnedActors().Where(x => !x.IsRemoved).Where(x => !x.IsDead).ToArray();
        // send actor data to player
        var packet = new ActorRequestSendPacket();
        var replications = owned.Select(ActorReplicationData.FromActor);
        packet.Actors.AddRange(replications);
        sessionManager.SendP2PPacket(sender.SteamId, NetChannel.GameState, packet);

        // actor data update
        foreach (var actor in owned)
        {
            sessionManager.SendP2PPacket(sender.SteamId, NetChannel.ActorAction, ActorActionPacket.CreateSetZonePacket(actor.ActorId, actor.Zone, actor.ZoneOwner));
            sessionManager.SendP2PPacket(sender.SteamId, NetChannel.ActorUpdate, ActorUpdatePacket.Create(actor));
        }

        await Task.Yield();
    }
}