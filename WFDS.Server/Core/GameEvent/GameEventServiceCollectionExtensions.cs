using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using WFDS.Common.GameEvents;

namespace WFDS.Server.Core.GameEvent;

internal static class GameEventServiceCollectionExtensions
{
    public static void AddGameEventHandlers(this IServiceCollection services)
    {
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var gameEventHandlerType = typeof(GameEventHandler);

        var gameEventHandlerTypes = allAssemblies
            .SelectMany(assembly => assembly.ExportedTypes)
            .Distinct()
            .Where(type => gameEventHandlerType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
            .ToArray();

        Log.Logger.Information("added {Count} game event handlers", gameEventHandlerTypes.Length);
        services.TryAddEnumerable(gameEventHandlerTypes.Select(CreateServiceDescriptor));
    }

    private static ServiceDescriptor CreateServiceDescriptor(Type type)
    {
        return new ServiceDescriptor(typeof(GameEventHandler), type, ServiceLifetime.Transient);
    }
}