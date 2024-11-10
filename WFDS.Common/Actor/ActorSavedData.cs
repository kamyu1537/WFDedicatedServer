namespace WFDS.Server.Common.Actor;

public record ActorSavedData(string ActorType, long ActorId, long OwnerId)
{
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