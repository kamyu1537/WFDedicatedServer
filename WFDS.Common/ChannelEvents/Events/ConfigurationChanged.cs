using WFDS.Common.Types;

namespace WFDS.Common.ChannelEvents.Events;

public class ConfigurationChanged(IServerSetting setting) : ChannelEvent
{
    public IServerSetting Setting { get; set; } = setting;
}