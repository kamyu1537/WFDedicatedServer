﻿using WFDS.Common.Actor;
using WFDS.Common.Extensions;
using WFDS.Common.Network;
using WFDS.Common.Types;

namespace WFDS.Common.Network.Packets;

public class ActorRequestSendPacket : IPacket
{
    public List<ActorReplicationData> Actors { get; set; } = [];

    public void Deserialize(Dictionary<object, object> data)
    {
        Actors = data.GetObjectList("list")
            .Select(x => PacketHelper.FromDictionary<ActorReplicationData>(x.GetObjectDictionary()))
            .ToList();
    }

    public void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "actor_request_send");
        data.TryAdd("list", Actors.Select(object (x) => x.ToDictionary()).ToList());
    }
}