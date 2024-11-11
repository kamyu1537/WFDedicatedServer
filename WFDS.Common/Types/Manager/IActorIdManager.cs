namespace WFDS.Common.Types.Manager;

public interface IActorIdManager
{
    bool Add(long id);
    bool Return(long id);
    long Next();
}