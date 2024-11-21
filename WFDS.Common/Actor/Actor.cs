using System.Numerics;
using Microsoft.Extensions.ObjectPool;
using Steamworks;

namespace WFDS.Common.Actor;

public abstract class Actor<T> : IActor where T : class, IActor, new()
{
    private static readonly ObjectPool<T> Pool = new DefaultObjectPool<T>(new DefaultPooledObjectPolicy<T>());
    
    public abstract ActorType Type { get; }
    public abstract long ActorId { get; set; }
    public abstract CSteamID CreatorId { get; set; }
    public abstract string Zone { get; set; }
    public abstract long ZoneOwner { get; set; }
    public abstract Vector3 Position { get; set; }
    public abstract Vector3 Rotation { get; set; }
    public abstract bool Decay { get; }
    public abstract long DecayTimer { get; set; }
    public abstract DateTimeOffset CreateTime { get; set; }
    public abstract bool CanWipe { get; }
    public abstract bool IsRemoved { get; set; }
    public abstract bool IsDead { get; set; }
    public abstract long NetworkShareDefaultCooldown { get; }
    public abstract long NetworkShareCooldown { get; set; }
    
    protected virtual void OnReset()
    {
    }
    
    protected virtual void OnRemoved()
    {
    }

    public void Reset()
    {
        ActorId = 0;
        CreatorId = CSteamID.Nil;

        Zone = string.Empty;
        ZoneOwner = -1;
        
        Position = Vector3.Zero;
        Rotation = Vector3.Zero;
        
        IsRemoved = false;
        OnReset();
    }
    
    public void Remove()
    {
        IsRemoved = true;
        OnRemoved();
        Return(this as T);
    }

    private static void Return(T? actor)
    {
        if (actor == null) return;
        Console.WriteLine("Return Actor: " + actor.GetType());
        Pool.Return(actor);
    }
    
    public static T Get()
    {
        var actor = Pool.Get();
        actor.Reset();
        return actor;
    }
}