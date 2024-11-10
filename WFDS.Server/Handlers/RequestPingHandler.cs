﻿using Steamworks;
using WFDS.Server.Common;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("request_ping")]
public class RequestPingHandler : PacketHandler
{
    public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
    {
        sender.PingReceiveTime = DateTimeOffset.UtcNow;
        sender.SendPacket(NetChannel.GameState, new SendPingPacket
        {
            FromId = SteamClient.SteamId
        });
    }
}