using WFDS.Server.Common;
using WFDS.Server.Common.Actor;

namespace WFDS.Server.Packets;

public class ActorRequestSendPacket : IPacket
{
    public List<ActorSavedData> Actors { get; set; } = [];

    public void Parse(Dictionary<object, object> data)
    {
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