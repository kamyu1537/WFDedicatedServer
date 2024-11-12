using WFDS.Common.Extensions;
using WFDS.Common.Types;
using WFDS.Server.Network;

namespace WFDS.Server.Packets;

/*
 * queue_free : 프롭 삭제
 */

public class ActorActionPacket : IPacket
{
    public long ActorId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public List<object> Params { get; private set; } = [];

    public void Deserialize(Dictionary<object, object> data)
    {
        ActorId = data.GetInt("actor_id");
        Action = data.GetString("action");
        Params = data.GetObjectList("params");
    }

    public void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "actor_action");
        data.TryAdd("actor_id", ActorId);
        data.TryAdd("action", Action);
        data.TryAdd("params", Params);
    }

    public static ActorActionPacket CreateWipeActorPacket(long actorId)
    {
        return new ActorActionPacket
        {
            ActorId = actorId,
            Action = "_wipe_actor",
            Params = [actorId]
        };
    }

    public static ActorActionPacket CreateQueueFreePacket(long actorId)
    {
        return new ActorActionPacket
        {
            ActorId = actorId,
            Action = "queue_free",
            Params = []
        };
    }
}