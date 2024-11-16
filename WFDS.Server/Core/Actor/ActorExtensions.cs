using WFDS.Common.Actor;

namespace WFDS.Server.Core.Actor;

public static class ActorExtensions
{
    public static dynamic ToDynamic(this IActor actor)
    {
        return new
        {
            actor.ActorId,
            Type = actor.Type.ToString(),
            CreatorId = actor.CreatorId.ToString(),
            actor.Zone,
            actor.ZoneOwner,
            actor.Position,
            actor.Rotation,
            actor.Decay,
            actor.DecayTimer,
            actor.CreateTime,
            actor.IsDead,
            actor.IsRemoved,
            actor.NetworkShareDefaultCooldown,
            actor.NetworkShareCooldown,
        };
    }
}