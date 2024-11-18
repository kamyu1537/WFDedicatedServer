using System.Globalization;
using Steamworks;
using WFDS.Common.Extensions;

namespace WFDS.Common.Network.Packets;

public class SendPingPacket : Packet
{
    public CSteamID FromId { get; set; } = CSteamID.Nil;

    public override void Deserialize(Dictionary<object, object> data)
    {
        FromId = data.GetParseULong("from").ToSteamID();
    }

    public override void Serialize(Dictionary<object, object> data)
    {
        var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        
        data.TryAdd("type", "send_ping");
        data.TryAdd("from", FromId.m_SteamID.ToString(CultureInfo.InvariantCulture));
        data.TryAdd("time", time);
    }
}