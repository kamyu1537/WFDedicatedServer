using WFDS.Godot.Types;
using WFDS.Server.Common;
using WFDS.Server.Common.Actor;
using WFDS.Server.Common.Extensions;
using WFDS.Server.Common.Network;
using WFDS.Server.Common.Packet;
using WFDS.Server.Managers;

namespace WFDS.Server.Packets;

public class ActorUpdatePacket : IPacket
{
    public long ActorId { get; set; }
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;

    public void Parse(Dictionary<object, object> data)
    {
        ActorId = data.GetNumber("actor_id");
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
    public static void SendUpdatePacket(this IActor actor, LobbyManager lobby)
    {
        var packet = new ActorUpdatePacket
        {
            ActorId = actor.ActorId,
            Position = actor.Position,
            Rotation = actor.Rotation
        };

        lobby.BroadcastPacket(NetChannel.ActorUpdate, packet, actor.Zone);
    }

    public static void SendUpdatePacket(this IActor actor, Session target)
    {
        var packet = new ActorUpdatePacket
        {
            ActorId = actor.ActorId,
            Position = actor.Position,
            Rotation = actor.Rotation
        };

        target.SendPacket(NetChannel.ActorUpdate, packet, actor.Zone);
    }
}