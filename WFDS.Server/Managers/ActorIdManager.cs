using WFDS.Common.Actor;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.Managers;

public sealed class ActorIdManager : IActorIdManager
{
    private readonly HashSet<long> _ids = [];
    private readonly Random _random = new();
    private readonly object _lock = new();

    public bool Add(long id)
    {
        lock (_lock)
        {
            return _ids.Add(id);
        }
    }

    public bool Return(long id)
    {
        lock (_lock)
        {
            return _ids.Remove(id);
        }
    }

    public long Next()
    {
        lock (_lock)
        {
            long id;
            do
            {
                id = _random.Next();
            } while (_ids.Contains(id));

            _ids.Add(id);
            return id;
        }
    }
}