using System.Globalization;
using WFDS.Common.Actor;
using WFDS.Common.Extensions;

namespace WFDS.Server.Core.Actor;

public static class ActorExtensions
{
    public static dynamic ToDynamic(this IActor actor)
    {
        return new
        {
            actor.ActorId,
            Type = actor.Type.ToString(),
            CreatorId = actor.CreatorId.m_SteamID.ToString(CultureInfo.InvariantCulture),
            actor.Zone,
            actor.ZoneOwner,
            Position = actor.Position.ToDynamic(),
            Rotation = actor.Rotation.ToDynamic(),
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