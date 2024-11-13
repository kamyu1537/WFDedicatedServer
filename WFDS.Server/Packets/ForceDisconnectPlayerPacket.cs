using Steamworks;
using WFDS.Common.Extensions;
using WFDS.Common.Types;

namespace WFDS.Server.Packets;

public class ForceDisconnectPlayerPacket : IPacket
{
    public SteamId UserId { get; set; }
    
    public void Deserialize(Dictionary<object, object> data)
    {
        UserId = data.GetParseULong("user_id");
    }

    public void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "force_disconnect_player");
        data.TryAdd("user_id", UserId.Value.ToString());
    }
}