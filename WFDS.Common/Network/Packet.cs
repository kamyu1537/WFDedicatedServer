namespace WFDS.Common.Network;

public abstract class Packet
{
    internal Packet()
    {
    }
    
    public abstract void Deserialize(Dictionary<object, object> data);
    public abstract void  Serialize(Dictionary<object, object> data);
}