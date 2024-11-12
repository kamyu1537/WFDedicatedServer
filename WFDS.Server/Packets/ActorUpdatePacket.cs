using WFDS.Common.Extensions;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Godot.Types;

namespace WFDS.Server.Packets;

public class ActorUpdatePacket : IPacket
{
    public long ActorId { get; set; }
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;

    public void Parse(Dictionary<object, object> data)
    {
        ActorId = data.GetInt("actor_id");
        Position = data.GetVector3("pos");
        Rotation = data.GetVector3("rot");
    }

    public Dictionary<object, object> ToDictionary()
    {
        return new Dictionary<object, object>
        {
            ["type"] = "actor_update",
            ["actor_id"] = ActorId,
            ["pos"] = Position,
            ["rot"] = Rotation
        };
    }
}

public static class ActorUpdatePacketExtensions
{
    public static void SendUpdatePacket(this IActor actor, ISessionManager lobby)
    {
        var packet = new ActorUpdatePacket
        {
            ActorId = actor.ActorId,
            Position = actor.Position,
            Rotation = actor.Rotation
        };

        lobby.BroadcastP2PPacket(NetChannel.ActorUpdate, packet, actor.Zone, actor.ZoneOwner);
    }

    public static void SendUpdatePacket(this IActor actor, ISession target)
    {
        var packet = new ActorUpdatePacket
        {
            ActorId = actor.ActorId,
            Position = actor.Position,
            Rotation = actor.Rotation
        };

        target.SendP2PPacket(NetChannel.ActorUpdate, packet, actor.Zone, actor.ZoneOwner);
    }
}