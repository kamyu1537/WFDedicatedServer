﻿using Steamworks;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.Network;

public abstract class PacketHandler : IPacketHandler
{
    protected string PacketType { get; private set; } = string.Empty;
    public ISessionManager? SessionManager { get; set; }
    public IActorManager? ActorManager { get; set; }

    protected ILogger Logger { get; private set; } = null!;

    public IPacketHandler Initialize(string packetType, ISessionManager lobbyManager, IActorManager actorManager, ILogger logger)
    {
        PacketType = packetType;
        SessionManager = lobbyManager;
        ActorManager = actorManager;
        Logger = logger;

        logger.LogInformation("initialized packet handler for {PacketType}", packetType);
        return this;
    }
    
    public abstract void HandlePacket(ISession sender, NetChannel channel, Dictionary<object, object> data);
    
    public void SendP2PPacket(SteamId target, NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1)
    {
        SessionManager?.SendP2PPacket(target, channel, packet, zone, zoneOwner);
    }

    public void BroadcastP2PPacket(NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1)
    {
        SessionManager?.BroadcastP2PPacket(channel, packet, zone, zoneOwner);
    }
}