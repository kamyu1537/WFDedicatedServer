using FSDS.Godot.Types;
using FSDS.Server.Common;
using FSDS.Server.Common.Actor;
using FSDS.Server.Common.Extensions;
using FSDS.Server.Managers;

namespace FSDS.Server.Packets;

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
    public static void SendActorUpdate(this IActor actor, LobbyManager lobby)
    {
        var packet = new ActorUpdatePacket
        {
            ActorId = actor.ActorId,
            Position = actor.Position,
            Rotation = actor.Rotation,
        };
        
        lobby.BroadcastPacket(NetChannel.ActorUpdate, packet);
    }
}