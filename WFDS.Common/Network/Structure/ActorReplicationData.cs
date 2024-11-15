using WFDS.Common.Extensions;
using WFDS.Common.Types;

namespace WFDS.Common.Actor;

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
    
    public void Deserialize(Dictionary<object, object> data)
    {
        ActorType = data.GetString("type");
        ActorId = data.GetInt("id");
        OwnerId = data.GetInt("owner_id");
    }

    public void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", ActorType);
        data.TryAdd("id", ActorId);
        data.TryAdd("owner_id", OwnerId);
    }
}