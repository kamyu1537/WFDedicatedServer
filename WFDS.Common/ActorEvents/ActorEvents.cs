using System.Numerics;
using WFDS.Common.Types;

namespace WFDS.Common.ActorEvents;

public record ActorCreateEvent(long ActorId) : IActorEvent;
public record ActorRemoveEvent(long ActorId) : IActorEvent;
public record ActorZoneUpdateEvent(long ActorId, string Zone, long ZoneOwner) : IActorEvent;
public record ActorUpdateEvent(long ActorId, Vector3 Position, Vector3 Rotation) : IActorEvent;
public record ActorTickEvent(long ActorId, double DeltaTime) : IActorEvent;

public record PlayerCosmeticsUpdateEvent(long ActorId, Cosmetics Cosmetics) : IActorEvent;
public record PlayerHeldItemUpdateEvent(long ActorId, GameItem Item) : IActorEvent;
public record PlayerMessageEvent(long ActorId, string Message, string Color, bool Local, Vector3 Position, string Zone, long ZoneOwner) : IActorEvent;
public record PlayerChatMessageEvent(long ActorId, string Message) : IActorEvent;
public record PlayerLevelUpEvent(long ActorId) : IActorEvent;