﻿using System.Text.Json;
using WFDS.Common.Extensions;
using WFDS.Common.Helpers;
using WFDS.Common.Types;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("actor_action")]
public class ActorActionHandler : PacketHandler
{
    private static readonly string[] AllowedActions =
    [
        "queue_free",
        "_wipe_actor",
        "_set_zone",
        "_update_cosmetics",
        "_update_held_item",
        "_sync_create_bubble",
        "_sync_level_bubble"
    ];

    public override void HandlePacket(ISession sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = PacketHelper.FromDictionary<ActorActionPacket>(data);

        if (!AllowedActions.Contains(packet.Action))
        {
            return;
        }

        Logger.LogInformation("received actor_action from {Member} for actor {ActorId} : {Action} / {Data}", sender.Friend, packet.ActorId, packet.Action, JsonSerializer.Serialize(packet.Params));

        QueueFree(sender, packet);
        WipeActor(sender, packet);
        SetZone(sender, packet);
        UpdateCosmetics(sender, packet);
        UpdateHeldItem(sender, packet);
        SyncCreateBubble(sender, packet);
        SyncLevelBubble(sender, packet);
    }

    private void QueueFree(ISession sender, ActorActionPacket packet)
    {
        if (packet.Action != "queue_free") return;
        
        if (packet.Params.Count != 0)
        {
            Logger.LogError("invalid queue_free packet from {Member} : {Data}", sender.Friend, JsonSerializer.Serialize(packet.Params));
            return;
        }

        ActorManager?.SelectActor(packet.ActorId, actor =>
        {
            if (actor.CreatorId == sender.SteamId)
            {
                ActorManager.TryRemoveActor(packet.ActorId, ActorRemoveTypes.QueueFree, out _);
            }
        });
    }

    private void WipeActor(ISession sender, ActorActionPacket packet)
    {
        if (packet.Action != "_wipe_actor") return;
        
        if (packet.Params.Count != 1)
        {
            Logger.LogError("invalid _wipe_actor packet from {Member} : {Data}", sender.Friend, JsonSerializer.Serialize(packet.Params));
            return;
        }

        var param = packet.Params[0];
        var actorId = param.GetNumber();

        ActorManager?.SelectActor(actorId, actor =>
        {
            if (actor.CreatorId == sender.SteamId)
            {
                ActorManager.TryRemoveActor(actorId, ActorRemoveTypes.WipeActor, out _);
            }
        });

        ActorManager?.SelectActor(packet.ActorId, actor =>
        {
            if (actor.CreatorId == sender.SteamId)
            {
                ActorManager.TryRemoveActor(actorId, ActorRemoveTypes.WipeActor, out _);
            }
        });
    }

    private void SetZone(ISession sender, ActorActionPacket packet)
    {
        if (packet.Action != "_set_zone") return;
        
        if (packet.Params.Count != 2)
        {
            Logger.LogError("invalid _set_zone packet from {Member} : {Data}", sender.Friend, JsonSerializer.Serialize(packet.Params));
            return;
        }

        if (!sender.ActorCreated)
        {
            Logger.LogError("actor not created for {Member}", sender.Friend);
            return;
        }

        ActorManager?.SelectActor(packet.ActorId, actor =>
        {
            if (actor.CreatorId != sender.SteamId)
                return;

            var zone = packet.Params[0].GetString();
            var zoneOwner = packet.Params[1].GetNumber();

            actor.OnZoneUpdated(zone, zoneOwner);
        });
    }

    private void UpdateCosmetics(ISession sender, ActorActionPacket packet)
    {
        if (packet.Action != "_update_cosmetics") return;
        
        if (packet.Params.Count != 1)
        {
            Logger.LogError("invalid _update_cosmetics packet from {Member} : {Data}", sender.Friend, JsonSerializer.Serialize(packet.Params));
            return;
        }

        ActorManager?.SelectPlayerActor(sender.SteamId, actor =>
        {
            if (actor.ActorId != packet.ActorId)
                return;

            var dic = packet.Params[0].GetObjectDictionary();
            var cosmetics = new Cosmetics();
            cosmetics.Parse(dic);
            actor.OnCosmeticsUpdated(cosmetics);
        });
    }

    private void UpdateHeldItem(ISession sender, ActorActionPacket packet)
    {
        if (packet.Action != "_update_held_item") return;
        
        if (packet.Params.Count != 1)
        {
            Logger.LogError("invalid _update_held_item packet from {Member} : {Data}", sender.Friend, JsonSerializer.Serialize(packet.Params));
            return;
        }

        ActorManager?.SelectPlayerActor(sender.SteamId, actor =>
        {
            if (actor.CreatorId != sender.SteamId)
                return;

            var dic = packet.Params[0].GetObjectDictionary();
            var item = new GameItem();
            item.Parse(dic);
            actor.OnHeldItemUpdated(item);
        });
    }

    private void SyncCreateBubble(ISession sender, ActorActionPacket packet)
    {
        if (packet.Action != "_sync_create_bubble") return;
        if (packet.Params.Count != 1)
        {
            Logger.LogError("invalid _sync_create_bubble packet from {Member} : {Data}", sender.Friend, JsonSerializer.Serialize(packet.Params));
            return;
        }
        
        ActorManager?.SelectPlayerActor(sender.SteamId, actor =>
        {
            if (actor.ActorId != packet.ActorId)
                return;

            var text = packet.Params[0].GetString();
            actor.OnChatMessage(text);
        });
    }
    
    private void SyncLevelBubble(ISession sender, ActorActionPacket packet)
    {
        if (packet.Action != "_sync_level_bubble") return;
        if (packet.Params.Count != 0)
        {
            Logger.LogError("invalid _sync_level_bubble packet from {Member} : {Data}", sender.Friend, JsonSerializer.Serialize(packet.Params));
            return;
        }
        
        ActorManager?.SelectPlayerActor(sender.SteamId, actor =>
        {
            if (actor.ActorId != packet.ActorId)
                return;

            actor.OnLevelUp();
        });
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