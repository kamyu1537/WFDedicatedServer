using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WFDS.Common.ChannelEvents;

public static class ChannelEventServiceCollectionExtensions
{
    public static void AddChannelEventHandlers(this IServiceCollection services)
    {
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var actorEventHandlerType = typeof(ChannelEventHandler);

        var actorEventHandlerTypes = allAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Distinct()
            .Where(type => actorEventHandlerType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface);

        services.TryAddEnumerable(actorEventHandlerTypes.Select(CreateServiceDescriptor));
    }

    private static ServiceDescriptor CreateServiceDescriptor(Type type)
    {
        return new ServiceDescriptor(typeof(ChannelEventHandler), type, ServiceLifetime.Transient);
    }
}