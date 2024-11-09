using WFDS.Server.Common;
using WFDS.Server.Common.Extensions;

namespace WFDS.Server.Packets;

public class RequestActorsPacket : IPacket
{
    public string UserId { get; set; } = string.Empty;
    
    public void Parse(Dictionary<object, object> data)
    {
        UserId = data.GetString("user_id");
    }

    public Dictionary<object, object> ToDictionary()
    {
        return new Dictionary<object, object>
        {
            ["type"] = "request_actors",
            ["user_id"] = UserId
        };
    }
}