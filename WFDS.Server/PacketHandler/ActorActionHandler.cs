using System.Text.Json;
using WFDS.Common.Actor;
using WFDS.Common.Extensions;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using ZLogger;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("actor_action")]
public class ActorActionHandler(ILogger<ActorActionHandler> logger, IActorManager actorManager, SteamManager steam) : PacketHandler<ActorActionPacket>
{
    protected override void Handle(Session sender, NetChannel channel, ActorActionPacket packet)
    {
        logger.LogDebug("received actor_action from {Member} for actor {ActorId} : {Action} / {Data}", sender.SteamId, packet.ActorId, packet.Action, JsonSerializer.Serialize(packet.Params));

        var actor = actorManager.GetActor(packet.ActorId);

        if (actor == null)
        {
            logger.ZLogWarning($"actor {packet.ActorId} not found for {sender}");
            return;
        }

        if (actor.CreatorId != sender.SteamId)
        {
            logger.ZLogWarning($"actor {packet.ActorId} not owned by {sender}");
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
            logger.ZLogError($"invalid queue_free packet from {sender} : {JsonSerializer.Serialize(packet.Params)}");
            return;
        }

        actorManager.TryRemoveActor(packet.ActorId, ActorRemoveTypes.QueueFree, out _);
    }

    private void WipeActor(Session sender, ActorActionPacket packet)
    {
        if (packet.Action != "_wipe_actor") return;

        if (packet.Params.Count != 1)
        {
            logger.ZLogError($"invalid _wipe_actor packet from {sender} : {JsonSerializer.Serialize(packet.Params)}");
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
            logger.ZLogError($"invalid _set_zone packet from {sender} : {JsonSerializer.Serialize(packet.Params)}");
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
            logger.ZLogError($"invalid _update_cosmetics packet from {sender} : {JsonSerializer.Serialize(packet.Params)}");
            return;
        }

        var player = actorManager.GetPlayerActor(sender.SteamId);
        if (player == null)
        {
            logger.ZLogError($"player actor not found for {sender}");
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
            logger.ZLogError($"invalid _update_held_item packet from {sender} : {JsonSerializer.Serialize(packet.Params)}");
            return;
        }

        var player = actorManager.GetPlayerActor(sender.SteamId);
        if (player == null)
        {
            logger.ZLogError($"player actor not found for {sender}");
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
            logger.ZLogError($"invalid _sync_create_bubble packet from {sender} : {JsonSerializer.Serialize(packet.Params)}");
            return;
        }

        var player = actorManager.GetPlayerActor(sender.SteamId);
        if (player == null)
        {
            logger.ZLogError($"player actor not found for {sender}", sender.ToString());
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
            logger.ZLogError($"invalid _sync_level_bubble packet from {sender} : {JsonSerializer.Serialize(packet.Params)}");
            return;
        }

        var player = actorManager.GetPlayerActor(sender.SteamId);
        if (player == null)
        {
            logger.ZLogError($"player actor not found for {sender}");
            return;
        }

        GameEventBus.Publish(new PlayerLevelUpEvent(sender.SteamId));
    }
}

/*
 * actor.queue_free()
 * actor._wipe_actor(number actor_id)
 * actor._set_zone(string current_zone, long current_zone_owner)
 * actor._change_id(Dictionary<object, object> data) - data: {"id": "empty", "ref": 0, "size": 1.0, "quality": QUALITY_NORMAL} - 아쿠아리움
 *
 * actor._sync_create_bubble(string text) - chat bubble
 * actor._sync_level_bubble() - level up bubble
 *
 * actor._sync_state(int state) - boombox
 * actor._flush() - toilet
 * actor._sync_punch()
 *
 * actor._sync_strum(string, fret, volume)
 * actor._sync_hammer(string, fret)
 *
 * actor._talk(string letter, float voice_pitch)
 * actor._face_emote(string emote)
 * actor._play_particle(string particle_id, Vector3 position, bool global)
 * actor._play_sfx(string sfx_id, Vector3 position, float pitch)
 *
 * actor._update_cosmetics(new) - fallback{"species": "species_cat", "pattern": "pattern_none", "primary_color": "pcolor_white", "secondary_color": "scolor_tan", "hat": "hat_none", "undershirt": "shirt_none", "overshirt": "overshirt_none", "title": "title_rank_1", "bobber": "bobber_default", "eye": "eye_halfclosed", "nose": "nose_cat", "mouth": "mouth_default", "accessory": [], "tail": "tail_cat", "legs": "legs_none"}
 * actor._update_held_item(RawItem held_item) - fallback {"id": "empty_hand", "ref": 0, "size": 1.0, "quality": ITEM_QUALITIES.NORMAL, "tags": []}
 */

/*
    const QUALITY_DATA = {
        ITEM_QUALITIES.NORMAL: {"color": "#d5aa73", "name": "", "diff": 1.0, "bdiff": 0.0, "worth": 1.0, "mod": "#ffffff", "op": 1.0, "particle": - 1, "title": "Normal "},
        ITEM_QUALITIES.SHINING: {"color": "#d5aa73", "name": "Shining ", "diff": 1.5, "bdiff": 3.0, "worth": 1.8, "mod": "#e5f5f0", "op": 1.0, "particle": 0, "title": "Shining "},
        ITEM_QUALITIES.GLISTENING: {"color": "#a49d9c", "name": "Glistening ", "diff": 2.5, "bdiff": 8.0, "worth": 4.0, "mod": "#eafcf5", "op": 1.0, "particle": 1, "title": "Glistening "},
        ITEM_QUALITIES.OPULENT: {"color": "#008583", "name": "Opulent ", "diff": 4.0, "bdiff": 14.0, "worth": 6.0, "mod": "#d5fcf5", "op": 1.0, "particle": 2, "title": "Opulent "},
        ITEM_QUALITIES.RADIANT: {"color": "#e69d00", "name": "Radiant ", "diff": 5.0, "bdiff": 24.0, "worth": 10.0, "mod": "#fcf0d5", "op": 1.0, "particle": 3, "title": "Radiant "},
        ITEM_QUALITIES.ALPHA: {"color": "#cd0462", "name": "Alpha ", "diff": 5.5, "bdiff": 32.0, "worth": 15.0, "mod": "#fcd5df", "op": 1.0, "particle": 4, "title": "Alpha "},
    }
*/