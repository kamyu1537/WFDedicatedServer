using System.Numerics;
using Steamworks;
using WFDS.Common.Types;

namespace WFDS.Common.ChannelEvents;

public record ActorCreateEvent(long ActorId) : IChannelEvent;
public record ActorRemoveEvent(long ActorId) : IChannelEvent;
public record ActorZoneUpdateEvent(long ActorId, string Zone, long ZoneOwner) : IChannelEvent;
public record ActorUpdateEvent(long ActorId, Vector3 Position, Vector3 Rotation) : IChannelEvent;
public record ActorTickEvent(long ActorId, double DeltaTime) : IChannelEvent;

public record PlayerCosmeticsUpdateEvent(SteamId PlayerId, Cosmetics Cosmetics) : IChannelEvent;
public record PlayerHeldItemUpdateEvent(SteamId PlayerId, GameItem Item) : IChannelEvent;
public record PlayerMessageEvent(SteamId PlayerId, string Message, string Color, bool Local, Vector3 Position, string Zone, long ZoneOwner) : IChannelEvent;
public record PlayerChatMessageEvent(SteamId PlayerId, string Message) : IChannelEvent;
public record PlayerLevelUpEvent(SteamId PlayerId) : IChannelEvent;
public record PlayerLeaveEvent(SteamId PlayerId) : IChannelEvent;