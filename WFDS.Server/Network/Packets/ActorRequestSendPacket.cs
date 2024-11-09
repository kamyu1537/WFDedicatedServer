using WFDS.Server.Common;

namespace WFDS.Server.Packets;

public class ActorRequestSendPacket : IPacket
{
    public void Parse(Dictionary<object, object> data)
    {
    }

    public Dictionary<object, object> ToDictionary()
    {
        return new Dictionary<object, object>
        {
            { "type", "actor_request_send" },
            { "list", new List<object>() }
        };
    }
}