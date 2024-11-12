using WFDS.Common.Extensions;

namespace WFDS.Common.Types;

public class ActorReplicationData : IPacket
{
    public string ActorType { get; set; } = string.Empty;
    public long ActorId { get; set; }
    public long OwnerId { get; set; }
    
    public static ActorReplicationData FromActor(IActor actor)
    {
        return new ActorReplicationData
        {
            ActorType = actor.ActorType,
            ActorId = actor.ActorId,
            OwnerId = (long)actor.CreatorId.Value
        };
    }
    
    public void Parse(Dictionary<object, object> data)
    {
        ActorType = data.GetString("type");
        ActorId = data.GetInt("id");
        OwnerId = data.GetInt("owner_id");
    }

    public Dictionary<object, object> ToDictionary()
    {
        return new Dictionary<object, object>
        {
            ["type"] = ActorType,
            ["id"] = ActorId,
            ["owner_id"] = OwnerId
        };
    }
}