using System.Reflection;
using WFDS.Common.Network;

namespace WFDS.Server.Core.Network;

public static class PacketHandlerExtensions
{
    public static IServiceCollection AddPacketHandlers(this IServiceCollection service)
    {
        var packetHandlerType = typeof(Common.Network.PacketHandler);
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Distinct();
        _ = types
            .Where(t => t.IsClass && !t.IsAbstract && packetHandlerType.IsAssignableFrom(t))
            .Select(t => (t.GetCustomAttribute<PacketTypeAttribute>(), t))
            .Where(x => x is { Item1: not null, Item2: not null })
            .Select(x => x.Item2)
            .Select(service.AddTransient).ToArray();

        return service;
    }
}