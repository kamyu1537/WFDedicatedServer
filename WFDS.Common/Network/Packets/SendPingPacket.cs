using Steamworks;
using WFDS.Common.Extensions;

namespace WFDS.Common.Network.Packets;

public class SendPingPacket : Packet
{
    public SteamId FromId { get; set; } = 0;

    public override void Deserialize(Dictionary<object, object> data)
    {
        FromId = data.GetParseULong("from");
    }

    public override void Serialize(Dictionary<object, object> data)
    {
        var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        
        data.TryAdd("type", "send_ping");
        data.TryAdd("from", FromId.Value.ToString());
        data.TryAdd("time", time);
    }
}