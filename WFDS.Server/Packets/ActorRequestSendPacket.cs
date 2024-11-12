using WFDS.Common.Extensions;
using WFDS.Common.Helpers;
using WFDS.Common.Types;

namespace WFDS.Server.Packets;

public class ActorRequestSendPacket : IPacket
{
    public List<ActorReplicationData> Actors { get; set; } = [];

    public void Parse(Dictionary<object, object> data)
    {
        Actors = data.GetObjectList("list")
            .Select(x => PacketHelper.FromDictionary<ActorReplicationData>(x.GetObjectDictionary()))
            .ToList();
    }

    public Dictionary<object, object> ToDictionary()
    {
        return new Dictionary<object, object>
        {
            { "type", "actor_request_send" },
            { "list", Actors.Select(object (x) => x.ToDictionary()).ToList() }
        };
    }
}