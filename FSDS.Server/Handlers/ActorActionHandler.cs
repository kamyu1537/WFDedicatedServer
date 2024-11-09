using System.Text.Json;
using FSDS.Server.Common;
using FSDS.Server.Common.Extensions;
using FSDS.Server.Packets;

namespace FSDS.Server.Handlers;

[PacketType("actor_action")]
public class ActorActionHandler : PacketHandler
{
    public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = new ActorActionPacket();
        packet.Parse(data);

        PrintReceiveLog(sender, packet);

        switch (packet.Action)
        {
            case "queue_free":
                QueueFree(sender, packet);
                break;
            case "_wipe_actor":
                WipeActor(sender, packet);
                break;
        }
    }

    private void PrintReceiveLog(Session sender, ActorActionPacket packet)
    {
        if (packet.Action != "queue_free" && packet.Action != "_wipe_actor")
        {
            return;
        }
        
        LobbyManager.SelectSession(sender.SteamId, session => { Logger.LogInformation("received actor_action from {Name}[{SteamId}] for actor {ActorId} : {Action} / {Data}", session.Friend.Name, sender.SteamId, packet.ActorId, packet.Action, JsonSerializer.Serialize(packet.Params)); });
    }

    private void QueueFree(Session sender, ActorActionPacket packet)
    {
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
        if (packet.Params.Count == 1)
        {
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
        else
        {
            Logger.LogError("invalid _wipe_actor packet from {Name}[{SteamId}] : {Data}", sender.Friend.Name, sender.SteamId, JsonSerializer.Serialize(packet.Params));
        }
    }
}