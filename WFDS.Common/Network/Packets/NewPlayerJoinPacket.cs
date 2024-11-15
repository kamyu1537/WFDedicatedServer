namespace WFDS.Common.Network.Packets;

public class NewPlayerJoinPacket : Packet
{
    public override void Deserialize(Dictionary<object, object> data)
    {
    }

    public override void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "new_player_join");
    }
}