namespace WFDS.Server.Common;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class PacketTypeAttribute(string packetType) : Attribute
{
    public string PacketType { get; } = packetType;
}