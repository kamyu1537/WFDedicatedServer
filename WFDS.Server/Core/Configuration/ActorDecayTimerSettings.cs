using System.Collections.ObjectModel;

namespace WFDS.Server.Core.Actor;

internal sealed class ActorDecayTimerSettings
{
    public int fish_spawn { get; set; } = 4800;
    public int fish_spawn_alien { get; set; } = 14400;
    public int raincloud { get; set; } = 32500;
    public int metal_spawn { get; set; } = 10000;
    public int ambient_bird { get; set; } = 600;
    public int void_portal { get; set; } = 36000;
}

internal static class ActorDecayTimerSettingsExtensions
{
    public static void UseDecayTimerConfigure(this IHostApplicationBuilder builder, IConfigurationRoot configuration)
    {
        var section = configuration.GetSection("ActorDecayTimer");
        builder.Services.Configure<ActorDecayTimerSettings>(section);
    }
}