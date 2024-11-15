﻿using WFDS.Common.Actor;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("instance_actor")]
internal class InstanceActorHandler(ILogger<InstanceActorHandler> logger, IActorManager actorManager, ISessionManager sessionManager) : PacketHandler<InstanceActorPacket>
{
    protected override async Task HandlePacketAsync(Session sender, NetChannel channel, InstanceActorPacket packet)
    {
        logger.LogDebug("received instance_actor from {Sender} on channel {Channel} / {ActorId} {ActorType} ", sender.SteamId, channel, packet.Param.ActorId, packet.Param.ActorType);
        
        if (packet.Param.ActorType == "player") CreatePlayerActor(sender, packet);
        else CreateRemoteActor(sender, packet);
        await Task.Yield();
    }

    private void CreateRemoteActor(Session sender, InstanceActorPacket packet)
    {
        var actorType = ActorType.GetActorType(packet.Param.ActorType);
        if (actorType == null)
        {
            logger.LogError("actor type not found {ActorType} : {Member}", packet.Param.ActorType, sender.Friend);
            return;
        }

        if (actorType.HostOnly)
        {
            logger.LogWarning("actor type {ActorType} is host only : {Member}", actorType.Name, sender.Friend);
            sessionManager.KickPlayer(sender.SteamId);
            return;
        }

        var created = actorManager.TryCreateRemoteActor(sender.SteamId, packet.Param.ActorId, actorType, packet.Param.Position, packet.Param.Rotation, out _);
        if (!created)
        {
            logger.LogError("failed to create remote actor {ActorId} {ActorType}", packet.Param.ActorId, packet.Param.ActorType);
        }
    }

    private void CreatePlayerActor(Session sender, InstanceActorPacket packet)
    {
        if (actorManager.GetPlayerActor(sender.SteamId) != null)
        {
            logger.LogError("player already has actor {Member} - {ActorId} {ActorType}", sender.Friend, packet.Param.ActorId, packet.Param.ActorType);
            return;
        }
        
        var created = actorManager.TryCreatePlayerActor(sender.SteamId, packet.Param.ActorId, out _);
        if (created) return;
        
        logger.LogError("failed to create player actor {ActorId} {ActorType}", packet.Param.ActorId, packet.Param.ActorType);
    }
}