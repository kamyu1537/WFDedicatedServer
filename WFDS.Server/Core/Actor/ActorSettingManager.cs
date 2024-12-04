using Microsoft.Extensions.Options;
using WFDS.Common.Actor;

namespace WFDS.Server.Core.Actor;

internal class ActorSettingManager(IOptions<ActorSettings> actorSettings) : IActorSettingManager
{   
    public int GetMaxCount(string typeName)
    {
        var maxCounts = actorSettings.Value.MaxCount;
        if (maxCounts.TryGetValue(typeName, out var count))
        {
            return count;
        }

        var actorType = ActorType.GetActorType(typeName);
        if (actorType != null)
        {
            return actorType.MaxCount;
        }

        return -1;
    }

    public int GetDecayTimer(string typeName)
    {
        var decayTimer = actorSettings.Value.DecayTimer;
        if (decayTimer.TryGetValue(typeName, out var timer))
        {
            return timer;
        }

        var actorType = ActorType.GetActorType(typeName);
        if (actorType != null)
        {
            return actorType.DecayTimer;
        }
        
        return -1;
    }
}