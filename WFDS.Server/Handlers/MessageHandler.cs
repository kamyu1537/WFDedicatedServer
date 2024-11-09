﻿using System.Drawing;
using WFDS.Server.Common;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("message")]
public class MessageHandler : PacketHandler
{
    public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = new MessagePacket();
        packet.Parse(data);

        Logger.LogInformation("received message from {Sender} ({Zone}/{ZoneOwner}) on channel {Channel} / [{Color}] {Message}", sender.SteamId, packet.Zone, packet.ZoneOwner, channel, packet.Color, packet.Message);
        // sender.SendMessage(packet.Message, Color.White); // test code
        // if (packet.Message == "%u: alien")
        // {
        //     ActorManager.SelectPlayerActor(sender.SteamId, actor =>
        //     {
        //         ActorManager.SpawnFish(actor.Position, "fish_spawn_alien");
        //     });
        // }
    }
}