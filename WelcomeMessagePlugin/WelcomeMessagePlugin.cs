using WFDS.Plugin;

namespace WelcomeMessagePlugin;

public class WelcomeMessagePlugin : Plugin
{
    public override string Name => "Welcome Message";
    public override string Author => "kamyu";
    public override string Version => "1.0.0";
    
    public override void Load()
    {
        RegisterEventHandler<WelcomeMessageEventHandler>();
    }
}