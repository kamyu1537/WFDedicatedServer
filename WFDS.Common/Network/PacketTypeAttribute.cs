namespace WFDS.Common.Network;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class PacketTypeAttribute(string packetType) : Attribute
{
    public string PacketType { get; } = packetType;
}