using System.Text.Json;
using WFDS.Server.Common;
using WFDS.Server.Common.Extensions;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("actor_action")]
public class ActorActionHandler : PacketHandler
{
    private static readonly string[] Actions =
    [
        "queue_free",
        "_wipe_actor",
        "_set_zone"
    ];
    
    public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = new ActorActionPacket();
        packet.Parse(data);

        if (!Actions.Contains(packet.Action))
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
                ActorManager.RemoveActor(packet.ActorId);
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
                ActorManager.RemoveActor(actorId);
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

        var zone = packet.Params[0].GetString();
        var zoneOwner = packet.Params[1].GetNumber();

        if (!sender.ActorCreated)
        {
            Logger.LogError("actor not created for {Name}[{SteamId}]", sender.Friend.Name, sender.SteamId);
            return;
        }

        var actor = sender.Actor;
        actor.Zone = zone;
        actor.ZoneOwner = zoneOwner;
    }
}