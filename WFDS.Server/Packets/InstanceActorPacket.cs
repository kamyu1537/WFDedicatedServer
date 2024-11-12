using WFDS.Common.Extensions;
using WFDS.Common.Helpers;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Godot.Types;

namespace WFDS.Server.Packets;

public class ActorParamData : IPacket
{
    public static readonly ActorParamData Default = new();
    
    public string ActorType { get; set; } = string.Empty;
    public long ActorId { get; set; }
    public long CreatorId { get; set; }
    
    public string Zone { get; set; } = string.Empty;
    public long ZoneOwner { get; set; }
    
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    
    public void Parse(Dictionary<object, object> data)
    {
        ActorType = data.GetString("actor_type");
        ActorId = data.GetInt("actor_id");
        CreatorId = data.GetInt("creator_id");

        Zone = data.GetString("zone");
        ZoneOwner = data.GetInt("zone_owner");

        Position = data.GetVector3("at");
        Rotation = data.GetVector3("rot");
    }
    
    public void Write(Dictionary<object, object> data)
    {
        data.TryAdd("actor_type", ActorType);
        data.TryAdd("actor_id", ActorId);
        data.TryAdd("creator_id", CreatorId);
        data.TryAdd("zone", Zone);
        data.TryAdd("zone_owner", ZoneOwner);
        data.TryAdd("at", Position);
        data.TryAdd("rot", Rotation);
    }
}

public class InstanceActorPacket : IPacket
{
    public ActorParamData Param { get; set; } = ActorParamData.Default;

    public void Parse(Dictionary<object, object> data)
    {
        Param = PacketHelper.FromDictionary<ActorParamData>(data.GetObjectDictionary("params"));
    }

    public void Write(Dictionary<object, object> data)
    {
        data.TryAdd("type", "instance_actor");
        data.TryAdd("params", Param.ToDictionary());
    }
}

public static class InstanceActorExtensions
{
    private static InstanceActorPacket ToInstancePacket(this IActor actor)
    {
        return new InstanceActorPacket
        {
            Param = new ActorParamData
            {
                ActorType = actor.ActorType,
                ActorId = actor.ActorId,
                CreatorId = (long)actor.CreatorId.Value,
                Zone = actor.Zone,
                ZoneOwner = actor.ZoneOwner,
                Position = actor.Position,
                Rotation = actor.Rotation
            }
        };
    }

    public static void SendInstanceActor(this IActor actor, ISessionManager session)
    {
        session.BroadcastP2PPacket(NetChannel.GameState, actor.ToInstancePacket());
    }
}