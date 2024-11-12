namespace WFDS.Common.Types;

public interface IPacket
{
    public void Deserialize(Dictionary<object, object> data);
    public void  Serialize(Dictionary<object, object> data);
}