namespace WFDS.Common.Types;

public interface IPacket
{
    public void Parse(Dictionary<object, object> data);
    public void  Write(Dictionary<object, object> data);
}