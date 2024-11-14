using WFDS.Common.Types;

namespace WFDS.Network.Packets;

public record BanPacket : IPacket
{
    public void Deserialize(Dictionary<object, object> data)
    {
    }

    public void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "ban");
    }
}