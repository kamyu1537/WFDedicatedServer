using Steamworks;
using WFDS.Common.Extensions;
using WFDS.Common.Types;
using WFDS.Server.Network;

namespace WFDS.Server.Packets;

public class SendPingPacket : IPacket
{
    public SteamId FromId { get; set; } = 0;

    public void Parse(Dictionary<object, object> data)
    {
        FromId = data.GetParseULong("from");
    }

    public void Write(Dictionary<object, object> data)
    {
        var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        
        data.TryAdd("type", "send_ping");
        data.TryAdd("from", FromId.Value.ToString());
        data.TryAdd("time", time);
    }
}