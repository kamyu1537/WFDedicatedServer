using System.Numerics;
using Steamworks;

namespace WFDS.Common.Actor;

public interface IActor
{
    ActorType Type { get; }
    long ActorId { get; set; }
    CSteamID CreatorId { get; set; }
    string Zone { get; set; }
    long ZoneOwner { get; set; }
    Vector3 Position { get; set; }
    Vector3 Rotation { get; set; }

    bool Decay { get; }
    long DefaultDecayTimer { get; }
    long DecayTimer { get; set; }
    DateTimeOffset CreateTime { get; set; }

    bool CanWipe { get; }
    bool IsRemoved { get; set; }
    bool IsDead { get; set; }
    
    long NetworkShareDefaultCooldown { get; }
    long NetworkShareCooldown { get; set; }

    void Reset();
    void Remove();
}