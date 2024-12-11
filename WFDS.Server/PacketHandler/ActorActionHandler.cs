using System.Text.Json;
using WFDS.Common.Actor;
using WFDS.Common.Extensions;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("actor_action")]
public sealed class ActorActionHandler(ILogger<ActorActionHandler> logger, IActorManager actorManager, SteamManager steam) : PacketHandler<ActorActionPacket>
{
    protected override void Handle(Session sender, NetChannel channel, ActorActionPacket packet)
    {
        logger.LogDebug("received actor_action from {Member} for actor {ActorId} : {Action} / {Data}", sender.SteamId, packet.ActorId, packet.Action, JsonSerializer.Serialize(packet.Params));

        var actor = actorManager.GetActor(packet.ActorId);

        if (actor == null)
        {
            logger.LogWarning("actor {ActorId} not found for {Sender}", packet.ActorId, sender);
            return;
        }

        if (actor.CreatorId != sender.SteamId)
        {
            logger.LogWarning("actor {ActorId} not owned by {Sender}", packet.ActorId, sender);
            return;
        }

        QueueFree(sender, packet);
        WipeActor(sender, packet);
        SetZone(sender, packet);
        UpdateCosmetics(sender, packet);
        UpdateHeldItem(sender, packet);
        SyncCreateBubble(sender, packet);
        SyncLevelBubble(sender, packet);
    }

    private void QueueFree(Session sender, ActorActionPacket packet)
    {
        if (packet.Action != "queue_free") return;
        if (packet.Params.Count != 0)
        {
            logger.LogError("invalid queue_free packet from {Sender} : {Params}", sender, JsonSerializer.Serialize(packet.Params));
            return;
        }

        actorManager.TryRemoveActor(packet.ActorId, ActorRemoveTypes.QueueFree, out _);
    }

    private void WipeActor(Session sender, ActorActionPacket packet)
    {
        if (packet.Action != "_wipe_actor") return;

        if (packet.Params.Count != 1)
        {
            logger.LogError("invalid _wipe_actor packet from {Sender} : {Params}", sender, JsonSerializer.Serialize(packet.Params));
            return;
        }

        var param = packet.Params[0];
        var actorId = param.GetInt();

        var actor = actorManager.GetActor(actorId);
        if (actor == null) return;

        if (actor.CreatorId == steam.SteamId && actor.CanWipe) actorManager.TryRemoveActor(actorId, ActorRemoveTypes.WipeActor, out _);
    }

    private void SetZone(Session sender, ActorActionPacket packet)
    {
        if (packet.Action != "_set_zone") return;

        if (packet.Params.Count != 2)
        {
            logger.LogError("invalid _set_zone packet from {Sender} : {Params}", sender, JsonSerializer.Serialize(packet.Params));
            return;
        }

        var actor = actorManager.GetActor(packet.ActorId);
        if (actor != null)
        {
            if (actor.CreatorId != sender.SteamId)
                return;

            var zone = packet.Params[0].GetString();
            var zoneOwner = packet.Params[1].GetInt();
            GameEventBus.Publish(new ActorZoneUpdateEvent(actor.ActorId, zone, zoneOwner));
        }
    }

    private void UpdateCosmetics(Session sender, ActorActionPacket packet)
    {
        if (packet.Action != "_update_cosmetics") return;

        if (packet.Params.Count != 1)
        {
            logger.LogError("invalid _update_cosmetics packet from {Sender} : {Params}", sender, JsonSerializer.Serialize(packet.Params));
            return;
        }

        var player = actorManager.GetPlayerActor(sender.SteamId);
        if (player == null)
        {
            logger.LogError("player actor not found for {Sender} : {Params}", sender, JsonSerializer.Serialize(packet.Params));
            return;
        }

        var dic = packet.Params[0].GetObjectDictionary();
        var cosmetics = new Cosmetics();
        cosmetics.Deserialize(dic);
        GameEventBus.Publish(new PlayerCosmeticsUpdateEvent(sender.SteamId, cosmetics));
    }

    private void UpdateHeldItem(Session sender, ActorActionPacket packet)
    {
        if (packet.Action != "_update_held_item") return;

        if (packet.Params.Count != 1)
        {
            logger.LogError("invalid _update_held_item packet from {Sender} : {Params}", sender, JsonSerializer.Serialize(packet.Params));
            return;
        }

        var player = actorManager.GetPlayerActor(sender.SteamId);
        if (player == null)
        {
            logger.LogError("player actor not found for {Sender} : {Params}", sender, JsonSerializer.Serialize(packet.Params));
            return;
        }

        var dic = packet.Params[0].GetObjectDictionary();
        var item = new GameItem();
        item.Deserialize(dic);
        GameEventBus.Publish(new PlayerHeldItemUpdateEvent(sender.SteamId, item));
    }

    private void SyncCreateBubble(Session sender, ActorActionPacket packet)
    {
        if (packet.Action != "_sync_create_bubble") return;
        if (packet.Params.Count != 1)
        {
            logger.LogError("invalid _sync_create_bubble packet from {Sender} : {Params}", sender, JsonSerializer.Serialize(packet.Params));
            return;
        }

        var player = actorManager.GetPlayerActor(sender.SteamId);
        if (player == null)
        {
            logger.LogError("player actor not found for {Sender} : {Params}", sender, JsonSerializer.Serialize(packet.Params));
            return;
        }

        var text = packet.Params[0].GetString();
        GameEventBus.Publish(new PlayerChatMessageEvent(sender.SteamId, text));
    }

    private void SyncLevelBubble(Session sender, ActorActionPacket packet)
    {
        if (packet.Action != "_sync_level_bubble") return;
        if (packet.Params.Count != 0)
        {
            logger.LogError("invalid _sync_level_bubble packet {Sender} : {Params}", sender, JsonSerializer.Serialize(packet.Params));
            return;
        }

        var player = actorManager.GetPlayerActor(sender.SteamId);
        if (player == null)
        {
            logger.LogError("player actor not found for {Sender}", sender);
            return;
        }

        GameEventBus.Publish(new PlayerLevelUpEvent(sender.SteamId));
    }
}