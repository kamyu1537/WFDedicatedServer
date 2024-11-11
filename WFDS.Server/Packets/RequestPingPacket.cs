using Steamworks;
using WFDS.Common.Extensions;
using WFDS.Common.Types;
using WFDS.Server.Network;

namespace WFDS.Server.Packets;

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
            ["sender"] = Sender.Value.ToString()
        };
    }
}