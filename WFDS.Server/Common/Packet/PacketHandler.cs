using System.Diagnostics.CodeAnalysis;
using WFDS.Server.Managers;
using WFDS.Server.Services;
using Steamworks;

namespace WFDS.Server.Common;

public interface IPacketHandler
{
    IPacketHandler Initialize(string packetType, LobbyManager lobbyManager, ActorManager actorManager, ILogger logger);
    void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data);
}

public abstract class PacketHandler : IPacketHandler
{
    protected string PacketType { get; private set; } = string.Empty;
    protected LobbyManager LobbyManager { get; private set; } = null!;   
    protected ActorManager ActorManager { get; private set; } = null!;

    protected ILogger Logger { get; private set; } = null!;
    
    public abstract void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data);
    
    public IPacketHandler Initialize(string packetType, LobbyManager lobbyManager, ActorManager actorManager, ILogger logger)
    {
        PacketType = packetType;
        LobbyManager = lobbyManager;
        ActorManager = actorManager;
        Logger = logger;
        
        logger.LogInformation("Initialized packet handler for {PacketType}", packetType);
        return this;
    }
    
    protected void Send(SteamId target, NetChannel channel, IPacket packet)
    {
        Send(target, channel, packet.ToDictionary());
    }
    
    protected void Send(SteamId target, NetChannel channel, Dictionary<object, object> data)
    {
        LobbyManager.SendPacket(target, channel, data);
    }
    
    protected void Broadcast(NetChannel channel, IPacket packet)
    {
        LobbyManager.BroadcastPacket(channel, packet.ToDictionary());
    }
    
    protected void Broadcast(NetChannel channel, Dictionary<object, object> data)
    {
        LobbyManager.BroadcastPacket(channel, data);
    }
}