namespace WFDS.Server.Common;

public interface IPacket
{
    public void Parse(Dictionary<object, object> data);
    public Dictionary<object, object> ToDictionary();
}