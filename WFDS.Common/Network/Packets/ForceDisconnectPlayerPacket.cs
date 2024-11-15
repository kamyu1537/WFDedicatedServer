using Steamworks;
using WFDS.Common.Extensions;

namespace WFDS.Common.Network.Packets;

public class ForceDisconnectPlayerPacket : Packet
{
    public SteamId UserId { get; set; }
    
    public override void Deserialize(Dictionary<object, object> data)
    {
        UserId = data.GetParseULong("user_id");
    }

    public override void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "force_disconnect_player");
        data.TryAdd("user_id", UserId.Value.ToString());
    }
}