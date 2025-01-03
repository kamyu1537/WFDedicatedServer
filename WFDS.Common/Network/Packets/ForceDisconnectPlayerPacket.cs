﻿using System.Globalization;
using Steamworks;
using WFDS.Common.Extensions;

namespace WFDS.Common.Network.Packets;

public class ForceDisconnectPlayerPacket : Packet
{
    public CSteamID UserId { get; set; }
    
    public override void Deserialize(Dictionary<object, object> data)
    {
        UserId = data.GetParseULong("user_id").ToSteamID();
    }

    public override void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "force_disconnect_player");
        data.TryAdd("user_id", UserId.m_SteamID.ToString(CultureInfo.InvariantCulture));
    }
}