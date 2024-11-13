using System.Reflection;
using System.Text.Json;
using Steamworks;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Server.Network;

namespace WFDS.Server.Managers;

public class PacketHandleManager : IPacketHandleManager
{
    private readonly ILogger<PacketHandleManager> _logger;
    private readonly Dictionary<string, IPacketHandler> _handlers;
    private readonly IGameSessionManager _session;

    public PacketHandleManager(
        ILogger<PacketHandleManager> logger,
        ILoggerFactory loggerFactory,
        IGameSessionManager session,
        IActorManager actor
    )
    {
        _logger = logger;
        _session = session;

        var packetHandlerType = typeof(IPacketHandler);
        var types = Assembly.GetExecutingAssembly().GetTypes(); 
        _handlers = types
            .Where(t => t.IsClass && !t.IsAbstract && packetHandlerType.IsAssignableFrom(t))
            .Select(t => (t.GetCustomAttribute<PacketTypeAttribute>(), Activator.CreateInstance(t) as IPacketHandler))
            .Where(x => x is { Item1: not null, Item2: not null })
            .Select(x => (x.Item1!.PacketType, x.Item2!))
            .Select(x => (x.Item1, x.Item2.Initialize(
                x.Item1,
                session,
                actor,
                loggerFactory.CreateLogger(x.Item2.GetType().Name
                )))).ToDictionary(x => x.Item1, x => x.Item2);
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
            _session.SelectSession(sender, session =>
            {
                session.PacketReceiveTime = DateTimeOffset.UtcNow;
                if (_handlers.TryGetValue(typeName, out var handler))
                {
                    handler.HandlePacket(session, channel, dic);
                }
            });
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