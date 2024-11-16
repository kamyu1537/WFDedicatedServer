using System.Globalization;
using Steamworks;
using WFDS.Common.Extensions;

namespace WFDS.Common.Network.Packets;

public class RequestPingPacket : Packet
{
    public SteamId Sender { get; set; } = 0;

    public override void Deserialize(Dictionary<object, object> data)
    {
        Sender = data.GetParseULong("sender");
    }

    public override void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "request_ping");
        data.TryAdd("sender", Sender.Value.ToString(CultureInfo.InvariantCulture));
    }
}