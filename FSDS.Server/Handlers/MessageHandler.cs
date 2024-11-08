using FSDS.Server.Common;
using FSDS.Server.Packets;

namespace FSDS.Server.Handlers;

[PacketType("message")]
public class MessageHandler : PacketHandler
{
    public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = new MessagePacket();
        packet.Parse(data);

        Logger.LogInformation("received message from {Sender} on channel {Channel} / {Message}", sender.SteamId, channel, packet.Message);

        // if (packet.Message == "%u: bird")
        // {
        //     ActorManager.SelectRandomPlayerActor(actor => { ActorManager.SpawnBird(actor.Position); });    
        // }
        // else if (packet.Message == "%u: fish")
        // {
        //     ActorManager.SelectRandomPlayerActor(actor => { ActorManager.SpawnFish(actor.Position); });
        // }
        // else if (packet.Message == "%u: alien")
        // {
        //     ActorManager.SelectRandomPlayerActor(actor => { ActorManager.SpawnFish(actor.Position, "fish_spawn_alien"); });
        // }
        // else if (packet.Message == "%u: void")
        // {
        //     ActorManager.SelectRandomPlayerActor(actor => { ActorManager.SpawnVoidPortal(actor.Position); });
        // }
        // else if (packet.Message == "%u: rain")
        // {
        //     ActorManager.SelectRandomPlayerActor(actor => { ActorManager.SpawnRainCloud(actor.Position); });
        // }
        // else if (packet.Message == "%u: metal")
        // {
        //     ActorManager.SelectRandomPlayerActor(actor => { ActorManager.SpawnMetal(actor.Position); });
        // }
    }
}