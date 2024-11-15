using WFDS.Common.Actor;
using WFDS.Common.Extensions;

namespace WFDS.Common.Network.Packets;

public class ActorRequestSendPacket : Packet
{
    public List<ActorReplicationData> Actors { get; set; } = [];

    public override void Deserialize(Dictionary<object, object> data)
    {
        Actors = data.GetObjectList("list")
            .Select(x => PacketHelper.FromDictionary<ActorReplicationData>(x.GetObjectDictionary()))
            .ToList();
    }

    public override void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "actor_request_send");
        data.TryAdd("list", Actors.Select(object (x) => x.ToDictionary()).ToList());
    }
}