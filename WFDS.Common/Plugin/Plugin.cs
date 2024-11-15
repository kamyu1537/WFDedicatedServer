using System.Collections.Immutable;
using WFDS.Common.ChannelEvents;
using WFDS.Common.Network;

namespace WFDS.Common.Plugin;

internal interface IPlugin
{
    public ImmutableArray<Type> EventHandlers { get; }
    public ImmutableArray<Type> PacketHandlers { get; }
    
    void Load();
    
    void RegisterEventHandler<T>() where T : ChannelEventHandler;
    void RegisterPacketHandler<T>() where T : PacketHandler;
}

public abstract class Plugin : IPlugin
{
    private readonly List<Type> _eventHandlers = new();
    private readonly List<Type> _packetHandlers = new();
    
    public abstract string Name { get; }
    public abstract string Author { get; }
    public abstract string Version { get; }
    
    public ImmutableArray<Type> EventHandlers => [.._eventHandlers.Distinct()];
    public ImmutableArray<Type> PacketHandlers => [.._packetHandlers.Distinct()];
    
    public abstract void Load();
    
    public void RegisterEventHandler<T>() where T : ChannelEventHandler
    {
        _eventHandlers.Add(typeof(T));
    }

    public void RegisterPacketHandler<T>() where T : PacketHandler
    {
        _packetHandlers.Add(typeof(T));
    }
}