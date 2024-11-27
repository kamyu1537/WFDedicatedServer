using System.Reflection;
using System.Text.Json;
using Steamworks;
using WFDS.Common.Network;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using ZLogger;

namespace WFDS.Server.Core.Network;

internal class PacketHandleManager
{
    private readonly ILogger<PacketHandleManager> _logger;
    private readonly IServiceProvider _provider;
    private readonly SessionManager _sessionManager;

    private readonly Dictionary<string, Type[]> _handlerTypes;

    public PacketHandleManager(ILogger<PacketHandleManager> logger, IServiceProvider provider, SessionManager sessionManager)
    {
        _logger = logger;
        _provider = provider;
        _sessionManager = sessionManager;

        var packetHandlerType = typeof(Common.Network.PacketHandler);
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetExportedTypes()).Distinct();
        _handlerTypes = types
            .Where(t => t.IsClass && !t.IsAbstract && packetHandlerType.IsAssignableFrom(t))
            .Select(t => (t.GetCustomAttribute<PacketTypeAttribute>(), t))
            .Where(x => x is { Item1: not null, Item2: not null })
            .GroupBy(x => x.Item1!.PacketType, x => x.Item2)
            .ToDictionary(x => x.Key, x => x.ToArray());
    }


    public void OnPacketReceived(CSteamID sender, NetChannel channel, object? data)
    {
        if (data is Dictionary<object, object> dic)
        {
            if (!dic.TryGetValue("type", out var type))
            {
                _logger.ZLogError($"received packet without type from {sender} on channel {channel}");
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
                _logger.ZLogWarning($"received packet from {sender} on channel {channel} without session");
                _sessionManager.KickPlayer(sender);
                return;
            }

            session.PacketReceiveTime = DateTimeOffset.UtcNow;
            if (!_handlerTypes.TryGetValue(typeName, out var handlerTypes)) return;
            foreach (var handler in handlerTypes.Select(handlerType => _provider.GetRequiredService(handlerType) as Common.Network.PacketHandler).Where(x => x != null))
            {
                handler!.Handle(session, channel, dic);
            }

            dic.Clear();
        }
        else
        {
            _logger.ZLogWarning($"received invalid packet from {sender} on channel {channel}");
        }
    }

    private void PrintDebugLog(string typeName, object data, CSteamID sender, NetChannel channel)
    {
        if (typeName == "actor_update" || typeName == "actor_action")
        {
            return;
        }

        var json = JsonSerializer.Serialize(data);
        _logger.LogDebug("received packet from {Sender} on channel {Channel} : {Data}", sender.m_SteamID, channel, json);
    }
}