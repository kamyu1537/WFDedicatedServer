using FSDS.Server.Common;
using FSDS.Server.Common.Extensions;
using Steamworks;

namespace FSDS.Server.Packets;

public class SendPingPacket : IPacket
{
    public SteamId FromId { get; set; } = 0;
    
    public void Parse(Dictionary<object, object> data)
    {
        FromId = data.GetParseULong("from");
    }

    public Dictionary<object, object> ToDictionary()
    {
        var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        
        return new Dictionary<object, object>
        {
            ["type"] = "send_ping",
            ["from"] = FromId.Value.ToString(),
            ["time"] = time
        };
    }
}