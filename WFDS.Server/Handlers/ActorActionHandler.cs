using System.Text.Json;
using WFDS.Common.Types;
using WFDS.Server.Common;
using WFDS.Server.Common.Extensions;
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
        "_update_held_item"
    ];

    public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = new ActorActionPacket();
        packet.Parse(data);

        if (!AllowedActions.Contains(packet.Action))
        {
            // Logger.LogInformation("received actor_action from {Name}[{SteamId}] for actor {ActorId} : {Action} / {Data}", sender.Friend.Name, sender.SteamId, packet.ActorId, packet.Action, JsonSerializer.Serialize(packet.Params));
            return;
        }

        Logger.LogInformation("received actor_action from {Name}[{SteamId}] for actor {ActorId} : {Action} / {Data}", sender.Friend.Name, sender.SteamId, packet.ActorId, packet.Action, JsonSerializer.Serialize(packet.Params));

        switch (packet.Action)
        {
            case "queue_free":
                QueueFree(sender, packet);
                break;
            case "_wipe_actor":
                WipeActor(sender, packet);
                break;
            case "_set_zone":
                SetZone(sender, packet);
                break;
            case "_update_cosmetics":
                UpdateCosmetics(sender, packet);
                break;
            case "_update_held_item":
                UpdateHeldItem(sender, packet);
                break;
        }
    }

    private void QueueFree(Session sender, ActorActionPacket packet)
    {
        if (packet.Params.Count != 0)
        {
            Logger.LogError("invalid queue_free packet from {Name}[{SteamId}] : {Data}", sender.Friend.Name, sender.SteamId, JsonSerializer.Serialize(packet.Params));
            return;
        }

        ActorManager.SelectActor(packet.ActorId, actor =>
        {
            if (actor.CreatorId == sender.SteamId)
            {
                ActorManager.RemoveActor(packet.ActorId, ActorRemoveTypes.QueueFree);
            }
        });
    }

    private void WipeActor(Session sender, ActorActionPacket packet)
    {
        if (packet.Params.Count != 1)
        {
            Logger.LogError("invalid _wipe_actor packet from {Name}[{SteamId}] : {Data}", sender.Friend.Name, sender.SteamId, JsonSerializer.Serialize(packet.Params));
            return;
        }

        var param = packet.Params[0];
        var actorId = param.GetNumber();

        ActorManager.SelectActor(actorId, actor =>
        {
            if (actor.CreatorId == sender.SteamId)
            {
                ActorManager.RemoveActor(actorId, ActorRemoveTypes.WipeActor);
            }
        });

        ActorManager.SelectActor(packet.ActorId, actor =>
        {
            if (actor.CreatorId == sender.SteamId)
            {
                ActorManager.RemoveActor(actorId, ActorRemoveTypes.WipeActor);
            }
        });
    }

    private void SetZone(Session sender, ActorActionPacket packet)
    {
        if (packet.Params.Count != 2)
        {
            Logger.LogError("invalid _set_zone packet from {Name}[{SteamId}] : {Data}", sender.Friend.Name, sender.SteamId, JsonSerializer.Serialize(packet.Params));
            return;
        }

        if (!sender.ActorCreated)
        {
            Logger.LogError("actor not created for {Name}[{SteamId}]", sender.Friend.Name, sender.SteamId);
            return;
        }

        ActorManager.SelectActor(packet.ActorId, actor =>
        {
            if (actor.CreatorId != sender.SteamId)
                return;

            var zone = packet.Params[0].GetString();
            var zoneOwner = packet.Params[1].GetNumber();

            actor.OnZoneUpdated(zone, zoneOwner);
        });
    }

    private void UpdateCosmetics(Session sender, ActorActionPacket packet)
    {
        if (packet.Params.Count != 1)
        {
            Logger.LogError("invalid _update_cosmetics packet from {Name}[{SteamId}] : {Data}", sender.Friend.Name, sender.SteamId, JsonSerializer.Serialize(packet.Params));
            return;
        }

        ActorManager.SelectActor(packet.ActorId, actor =>
        {
            if (actor.CreatorId != sender.SteamId)
                return;

            var dic = packet.Params[0].GetObjectDictionary();
            var cosmetics = new Cosmetics();
            cosmetics.Parse(dic);
            actor.OnCosmeticsUpdated(cosmetics);
        });
    }

    private void UpdateHeldItem(Session sender, ActorActionPacket packet)
    {
        if (packet.Params.Count != 1)
        {
            Logger.LogError("invalid _update_held_item packet from {Name}[{SteamId}] : {Data}", sender.Friend.Name, sender.SteamId, JsonSerializer.Serialize(packet.Params));
            return;
        }

        ActorManager.SelectActor(packet.ActorId, actor =>
        {
            if (actor.CreatorId != sender.SteamId)
                return;

            var dic = packet.Params[0].GetObjectDictionary();
            var item = new GameItem();
            item.Parse(dic);
            actor.OnHeldItemUpdated(item);
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
 * actor._play_particle(string particle_id, Vector3 position, bool global) - maybe can call server
 * actor._play_sfx(string sfx_id, Vector3 position, float pitch) - maybe can call server
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