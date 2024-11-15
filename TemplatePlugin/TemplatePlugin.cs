using WFDS.Common.Plugin;

namespace TemplatePlugin;

public class TemplatePlugin : Plugin
{
    public override string Name => "TemplatePlugin";
    public override string Author => "kamyu";
    public override string Version => "1.0.0";
    
    public override void Load()
    {
        RegisterEventHandler<PlayerJoinEventHandler>();
    }
}