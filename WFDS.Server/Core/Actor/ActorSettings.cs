using System.Collections.ObjectModel;

namespace WFDS.Server.Core.Actor;

public class ActorSettings
{
    public IReadOnlyDictionary<string, int> MaxCount { get; set; } = new Dictionary<string, int>();
    public IReadOnlyDictionary<string, int> DecayTimer { get; set; } = new Dictionary<string, int>();
}

internal static class ActorSettingsExtensions
{
    public static void UseActorSettings(this IHostApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection("Actor");
        builder.Services.Configure<ActorSettings>(section);
    }
}