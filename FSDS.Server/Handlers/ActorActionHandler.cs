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

        if (packet.Action == "queue_free")
        {
            LobbyManager.SessionExecute(sender.SteamId, session => { Logger.LogInformation("received actor_action from {Name}[{SteamId}] for actor {ActorId} : {Action} / {Data}", session.Friend.Name, sender.SteamId, packet.ActorId, packet.Action, JsonSerializer.Serialize(packet.Params)); });

            ActorManager.UpdateActor(packet.ActorId, actor =>
            {
                if (actor.CreatorId == sender.SteamId)
                {
                    ActorManager.RemoveActor(packet.ActorId);
                }
            });
        }

        // LobbyManager.SessionExecute(sender.SteamId, session => { Logger.LogInformation("received actor_action from {Name}[{SteamId}] for actor {ActorId} : {Action} / {Data}", session.Friend.Name, sender.SteamId, packet.ActorId, packet.Action, JsonSerializer.Serialize(packet.Params)); });
        //
        // if (packet.Action == "_update_held_item")
        // {
        //     if (packet.Params[0] is Dictionary<object, object> packetParam)
        //     {
        //         var itemId = packetParam.GetString("id");
        //         var refId = packetParam.GetNumber("ref");
        //         if (itemId == "fish_alien_dog")
        //         {
        //             ActorManager.UpdateActor(refId, actor => { Console.WriteLine("actorType: " + actor.ActorType); });
        //         }
        //     }
        // }
    }
}