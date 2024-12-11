using System.Collections.ObjectModel;

namespace WFDS.Server.Core.Actor;

internal sealed class ActorMaxCountSettings
{
    public int fish_spawn { get; set; } = 13;
    public int fish_spawn_alien { get; set; } = 1;
    public int raincloud { get; set; } = 1;
    public int metal_spawn { get; set; } = 8;
    public int ambient_bird { get; set; } = 8;
    public int void_portal { get; set; } = 1;
}

internal static class ActorSettingsExtensions
{
    public static void UseMaxCountConfigure(this IHostApplicationBuilder builder, IConfigurationRoot configuration)
    {
        var section = configuration.GetSection("ActorMaxCount");
        builder.Services.Configure<ActorMaxCountSettings>(section);
    }
}