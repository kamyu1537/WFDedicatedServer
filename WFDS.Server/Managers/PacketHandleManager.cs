using System.Reflection;
using System.Text.Json;
using Steamworks;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Server.Network;

namespace WFDS.Server.Managers;

public static class PacketHandlerExtensions
{
    public static IServiceCollection AddPacketHandlers(this IServiceCollection service)
    {
        var packetHandlerType = typeof(IPacketHandler);
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Distinct();
        _ = types
            .Where(t => t.IsClass && !t.IsAbstract && packetHandlerType.IsAssignableFrom(t))
            .Select(t => (t.GetCustomAttribute<PacketTypeAttribute>(), t))
            .Where(x => x is { Item1: not null, Item2: not null })
            .Select(x => x.Item2)
            .Select(x => service.AddTransient(x)).ToArray();

        return service;
    }
}

public class PacketHandleManager : IPacketHandleManager
{
    private readonly ILogger<PacketHandleManager> _logger;
    private readonly IGameSessionManager _sessionManager;
    private readonly IServiceProvider _provider;

    private readonly Dictionary<string, Type[]> _handlerTypes;

    public PacketHandleManager(ILogger<PacketHandleManager> logger, IServiceProvider provider, IGameSessionManager sessionManager)
    {
        _logger = logger;
        _provider = provider;
        _sessionManager = sessionManager;

        var packetHandlerType = typeof(IPacketHandler);
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Distinct();
        _handlerTypes = types
            .Where(t => t.IsClass && !t.IsAbstract && packetHandlerType.IsAssignableFrom(t))
            .Select(t => (t.GetCustomAttribute<PacketTypeAttribute>(), t))
            .Where(x => x is { Item1: not null, Item2: not null })
            .GroupBy(x => x.Item1!.PacketType, x => x.Item2)
            .ToDictionary(x => x.Key, x => x.ToArray());
    }


    public void OnPacketReceived(SteamId sender, NetChannel channel, object? data)
    {
        if (data is Dictionary<object, object> dic)
        {
            if (!dic.TryGetValue("type", out var type))
            {
                _logger.LogError("received packet without type from {Sender} on channel {Channel}", sender, channel);
                return;
            }

            if (type is not string typeName)
            {
                return;
            }

            PrintDebugLog(typeName, dic, sender, channel);
            var session = _sessionManager.GetSession(sender);
            if (session == null)
            {
                _logger.LogWarning("received packet from {Sender} on channel {Channel} without session", sender, channel);
                return;
            }

            session.PacketReceiveTime = DateTimeOffset.UtcNow;
            if (!_handlerTypes.TryGetValue(typeName, out var handlerTypes)) return;

            Task.WhenAll(handlerTypes
                .Select(handlerType => _provider.GetRequiredService(handlerType) as IPacketHandler)
                .Where(x => x != null)
                .Select(x =>
                {
                    x!.Initialize(_sessionManager);
                    return x.HandlePacketAsync(session, channel, dic);
                })).Wait();

            dic.Clear();
        }
        else
        {
            _logger.LogWarning("received invalid packet from {Sender} on channel {Channel}", sender, channel);
        }
    }

    private void PrintDebugLog(string typeName, object data, SteamId sender, NetChannel channel)
    {
        if (typeName == "actor_update" || typeName == "actor_action")
        {
            return;
        }

        var json = JsonSerializer.Serialize(data);
        _logger.LogDebug("received packet from {Sender} on channel {Channel} : {Data}", sender, channel, json);
    }
}