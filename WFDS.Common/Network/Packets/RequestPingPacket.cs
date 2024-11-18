using System.Globalization;
using Steamworks;
using WFDS.Common.Extensions;

namespace WFDS.Common.Network.Packets;

public class RequestPingPacket : Packet
{
    public CSteamID Sender { get; set; } = CSteamID.Nil;

    public override void Deserialize(Dictionary<object, object> data)
    {
        Sender = data.GetParseULong("sender").ToSteamID();
    }

    public override void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "request_ping");
        data.TryAdd("sender", Sender.m_SteamID.ToString(CultureInfo.InvariantCulture));
    }
}