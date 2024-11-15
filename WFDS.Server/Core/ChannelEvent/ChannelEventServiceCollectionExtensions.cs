using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using WFDS.Common.ChannelEvents;

namespace WFDS.Server.Core.ChannelEvent;

internal static class ChannelEventServiceCollectionExtensions
{
    public static void AddChannelEventHandlers(this IServiceCollection services)
    {
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var actorEventHandlerType = typeof(ChannelEventHandler);

        var actorEventHandlerTypes = allAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Distinct()
            .Where(type => actorEventHandlerType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
            .ToArray();

        Log.Logger.Information("added {Count} channel event handlers", actorEventHandlerTypes.Length);
        services.TryAddEnumerable(actorEventHandlerTypes.Select(CreateServiceDescriptor));
    }

    private static ServiceDescriptor CreateServiceDescriptor(Type type)
    {
        return new ServiceDescriptor(typeof(ChannelEventHandler), type, ServiceLifetime.Transient);
    }
}