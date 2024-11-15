using System.Numerics;
using WFDS.Common.Actor;
using WFDS.Common.ChannelEvents;
using WFDS.Common.Extensions;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Common.Network.Packets;

public class ActorUpdatePacket : IPacket
{
    public long ActorId { get; set; }
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;

    public void Deserialize(Dictionary<object, object> data)
    {
        ActorId = data.GetInt("actor_id");
        Position = data.GetVector3("pos");
        Rotation = data.GetVector3("rot");
    }

    public void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "actor_update");
        data.TryAdd("actor_id", ActorId);
        data.TryAdd("pos", Position);
        data.TryAdd("rot", Rotation);
    }
    
    public static ActorUpdatePacket Create(IActor actor)
    {
        return new ActorUpdatePacket
        {
            ActorId = actor.ActorId,
            Position = actor.Position,
            Rotation = actor.Rotation
        };
    }
}