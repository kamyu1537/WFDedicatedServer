using WFDS.Common.Types;

namespace WFDS.Common.GameEvents.Events;

public class ConfigurationChanged(IServerSetting setting) : GameEvent
{
    public IServerSetting Setting { get; set; } = setting;
}