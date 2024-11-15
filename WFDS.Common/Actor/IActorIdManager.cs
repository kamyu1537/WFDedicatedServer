namespace WFDS.Common.Actor;

public interface IActorIdManager
{
    bool Add(long id);
    bool Return(long id);
    long Next();
}