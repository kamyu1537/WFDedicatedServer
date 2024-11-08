using FSDS.Server.Common;
using FSDS.Server.Common.Extensions;
using Steamworks;

namespace FSDS.Server.Packets;

public class RequestPingPacket : IPacket
{
    public SteamId Sender { get; set; } = 0;
    
    public void Parse(Dictionary<object, object> data)
    {
        Sender = data.GetParseULong("sender");
    }

    public Dictionary<object, object> ToDictionary()
    {
        return new Dictionary<object, object>
        {
            ["type"] = "request_ping",
            ["sender"] = Sender.Value.ToString(),
        };
    }
}