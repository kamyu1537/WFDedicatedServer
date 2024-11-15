﻿using WFDS.Common.Actor;
using WFDS.Common.ChannelEvents.Events;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Server.Core.ChannelEvent;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("actor_update")]
internal class ActorUpdateHandler(IActorManager actorManager) : PacketHandler<ActorUpdatePacket>
{
    protected override async Task HandlePacketAsync(Session sender, NetChannel channel, ActorUpdatePacket packet)
    {
        var actor = actorManager.GetActor(packet.ActorId);
        if (actor == null) return;
        
        if (actor.CreatorId != sender.SteamId)
        {
            return;
        }
        
        await ChannelEventBus.PublishAsync(new ActorTransformUpdateEvent(actor.ActorId, packet.Position, packet.Rotation));
    }
}