﻿using WFDS.Common.Extensions;

namespace WFDS.Common.Network.Packets;

public class PeerWasKickedPacket : Packet
{
    public long UserId { get; set; }
    
    public override void Deserialize(Dictionary<object, object> data)
    {
        UserId = data.GetInt("user_id");
    }

    public override void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "peer_was_kicked");
        data.TryAdd("user_id", UserId);
    }
}