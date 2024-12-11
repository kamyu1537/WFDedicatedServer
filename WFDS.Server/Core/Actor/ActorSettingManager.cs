using Microsoft.Extensions.Options;
using WFDS.Common.Actor;

namespace WFDS.Server.Core.Actor;

internal class ActorSettingManager(
    IOptions<ActorMaxCountSettings> maxCount,
    IOptions<ActorDecayTimerSettings> decayTimer
) : IActorSettingManager
{
    private readonly Dictionary<string, int> _maxCounts = new()
    {
        { "fish_spawn", maxCount.Value.fish_spawn },
        { "fish_spawn_alien", maxCount.Value.fish_spawn_alien },
        { "raincloud", maxCount.Value.raincloud },
        { "metal_spawn", maxCount.Value.metal_spawn },
        { "ambient_bird", maxCount.Value.ambient_bird },
        { "void_portal", maxCount.Value.void_portal },
    };

    private readonly Dictionary<string, int> _decayTimer = new()
    {
        { "fish_spawn", decayTimer.Value.fish_spawn },
        { "fish_spawn_alien", decayTimer.Value.fish_spawn_alien },
        { "raincloud", decayTimer.Value.raincloud },
        { "metal_spawn", decayTimer.Value.metal_spawn },
        { "ambient_bird", decayTimer.Value.ambient_bird },
        { "void_portal", decayTimer.Value.void_portal },
    };

    public int GetMaxCount(string typeName)
    {
        if (_maxCounts.TryGetValue(typeName, out var count))
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
        if (_decayTimer.TryGetValue(typeName, out var timer))
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