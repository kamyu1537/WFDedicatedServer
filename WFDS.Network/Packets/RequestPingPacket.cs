using Steamworks;
using WFDS.Common.Extensions;
using WFDS.Common.Types;

namespace WFDS.Network.Packets;

public class RequestPingPacket : IPacket
{
    public SteamId Sender { get; set; } = 0;

    public void Deserialize(Dictionary<object, object> data)
    {
        Sender = data.GetParseULong("sender");
    }

    public void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "request_ping");
        data.TryAdd("sender", Sender.Value.ToString());
    }
}