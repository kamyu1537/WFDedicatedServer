using Steamworks;
using WFDS.Common.Extensions;
using WFDS.Common.Types;

namespace WFDS.Server.Packets;

public class RequestPingPacket : IPacket
{
    public SteamId Sender { get; set; } = 0;

    public void Parse(Dictionary<object, object> data)
    {
        Sender = data.GetParseULong("sender");
    }

    public Action? Write(Dictionary<object, object> data)
    {
        data.TryAdd("type", "request_ping");
        data.TryAdd("sender", Sender.Value.ToString());

        return null;
    }
}