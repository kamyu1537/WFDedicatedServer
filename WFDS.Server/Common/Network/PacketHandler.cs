using Steamworks;
using WFDS.Server.Common.Network;
using WFDS.Server.Managers;

namespace WFDS.Server.Common.Packet;

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

    protected void SendPacket(SteamId target, NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1)
    {
        SendPacket(target, channel, packet.ToDictionary(), zone, zoneOwner);
    }

    protected void SendPacket(SteamId target, NetChannel channel, Dictionary<object, object> data, string zone, long zoneOwner = -1)
    {
        LobbyManager.SendPacket(target, channel, data, zone, zoneOwner);
    }

    protected void Broadcast(NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1)
    {
        LobbyManager.BroadcastPacket(channel, packet.ToDictionary(), zone, zoneOwner);
    }

    protected void Broadcast(NetChannel channel, Dictionary<object, object> data, string zone = "", long zoneOwner = -1)
    {
        LobbyManager.BroadcastPacket(channel, data, zone, zoneOwner);
    }
}