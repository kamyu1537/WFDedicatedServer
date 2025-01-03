﻿using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("new_player_join")]
public sealed class NewPlayerJoinHandler(ILogger<NewPlayerJoinHandler> logger, ICanvasManager canvas, SessionManager session) : PacketHandler<NewPlayerJoinPacket>
{
    protected override void Handle(Session sender, NetChannel channel, NewPlayerJoinPacket packet)
    {
        logger.LogDebug("received new_player_join from {Sender} on channel {Channel}", sender.SteamId, channel);
        GameEventBus.Publish(new NewPlayerJoinEvent(sender.SteamId));

        var chalks = canvas.GetCanvasPackets();
        foreach (var chalk in chalks)
        {
            session.SendPacket(sender.SteamId, NetChannel.Chalk, chalk);
        }
    }
}