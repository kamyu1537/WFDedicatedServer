namespace WFDS.Common.Actor;

public interface IActorSettingManager
{
    int GetMaxCount(string typeName);
    int GetDecayTimer(string typeName);
}