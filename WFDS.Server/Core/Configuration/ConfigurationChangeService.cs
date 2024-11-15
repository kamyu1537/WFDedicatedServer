﻿using Microsoft.Extensions.Options;
using WFDS.Common.ChannelEvents.Events;
using WFDS.Common.Types.Manager;
using WFDS.Server.Core.ChannelEvent;

namespace WFDS.Server.Core.Configuration;

internal class ConfigurationChangeService(
    ILogger<ConfigurationChangeService> logger,
    IOptionsMonitor<ServerSetting> optionsMonitor,
    ISessionManager session
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
        ChannelEventBus.PublishAsync(new ConfigurationChanged(setting)).Wait();
        
        logger.LogInformation("reload banned players list: {Array}", string.Join(',', setting.BannedPlayers));
        session.BanPlayers(setting.BannedPlayers);
    }
}