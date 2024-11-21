namespace WFDS.Common.Types;

public class Singleton<T> where T : class, new()
{
    private static readonly Lazy<T> LazyInstance = new(() => new T());
    public static T Inst => LazyInstance.Value;
}