using Microsoft.Extensions.Options;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Steam;
using WFDS.Server.Core.Network;
using ZLogger;

namespace WFDS.Server.Core.Configuration;

internal class ConfigurationChangeService(
    ILogger<ConfigurationChangeService> logger,
    IOptionsMonitor<ServerSetting> optionsMonitor,
    LobbyManager lobby,
    SessionManager session
) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        optionsMonitor.OnChange(OnConfigurationChange);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void OnConfigurationChange(ServerSetting setting)
    {
        GameEventBus.Publish(new ConfigurationChanged(setting));
        
        logger.ZLogInformation($"reload banned players list: {string.Join(',', setting.BannedPlayers)}");
        session.BanPlayers(lobby.GetLobbyId(), setting.BannedPlayers);
    }
}