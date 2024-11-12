using Microsoft.Extensions.ObjectPool;

namespace WFDS.Common.Policies;

public class DictionaryPooledObjectPolicy : PooledObjectPolicy<Dictionary<object, object>>
{
    public override Dictionary<object, object> Create() => new();
    public override bool Return(Dictionary<object, object> obj)
    {
        obj.Clear();
        return true;
    }
}