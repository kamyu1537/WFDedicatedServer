using FSDS.Godot.Types;
using FSDS.Server.Common;
using FSDS.Server.Common.Actor;
using FSDS.Server.Common.Extensions;
using FSDS.Server.Common.Types;
using FSDS.Server.Managers;

namespace FSDS.Server.Packets;

public class InstanceActorPacket : IPacket
{
    public string ActorType { get; set; } = string.Empty;
    public long ActorId { get; set; }
    public long CreatorId { get; set; }
    
    public string Zone { get; set; } = string.Empty;
    public long ZoneOwner { get; set; }
    
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;

    public void Parse(Dictionary<object, object> data)
    {
        var param = data.GetObjectDictionary("params");

        ActorType = param.GetString("actor_type");        
        ActorId = param.GetNumber("actor_id");
        CreatorId = param.GetLong("creator_id");

        Zone = param.GetString("zone");
        ZoneOwner = param.GetInt("zone_owner");

        Position = param.GetVector3("at");
        Rotation = param.GetVector3("rot");
    }

    public Dictionary<object, object> ToDictionary()
    {
        return new Dictionary<object, object>
        {
            { "type", "instance_actor" },
            {
                "params", new Dictionary<object, object>
                {
                    { "actor_type", ActorType },
                    { "actor_id", ActorId },
                    { "creator_id", CreatorId },
                    { "zone", Zone },
                    { "zone_owner", ZoneOwner },
                    { "at", Position },
                    { "rot", Rotation }
                }
            }
        };
    }
}

public static class InstanceActorExtensions
{
    private static InstanceActorPacket ToPacket(this IActor actor)
    {
        return new InstanceActorPacket
        {
            ActorType = actor.ActorType,
            ActorId = actor.ActorId,
            CreatorId = (long)actor.CreatorId.Value,
            Zone = actor.Zone,
            ZoneOwner = actor.ZoneOwner,
            Position = actor.Position,
            Rotation = actor.Rotation
        };
    }
    
    public static void SendInstanceActor(this IActor actor, LobbyManager lobby)
    {
        lobby.BroadcastPacket(NetChannel.GameState, actor.ToPacket());
    }
    
    public static void SendInstanceActor(this IActor actor, Session target)
    {
        target.Send(NetChannel.GameState, actor.ToPacket());
    }
}