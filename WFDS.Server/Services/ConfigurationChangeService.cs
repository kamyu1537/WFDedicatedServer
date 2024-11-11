using WFDS.Common.Types.Manager;

namespace WFDS.Server.Services;

public class ConfigurationChangeService(
    ILogger<ConfigurationChangeService> logger,
    IConfiguration configuration,
    ISessionManager session
) : IHostedService
{
    private IDisposable? _reloadToken;
    private bool _disposed;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        RegisterConfigurationChangeCallback();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _reloadToken?.Dispose();
        return Task.CompletedTask;
    }

    private void RegisterConfigurationChangeCallback()
    {
        _reloadToken?.Dispose();
        _disposed = true;

        _reloadToken = configuration.GetReloadToken().RegisterChangeCallback(OnConfigurationChange, null);
    }

    private void OnConfigurationChange(object? obj)
    {
        if (!_disposed) return;
        _disposed = false;
        RegisterConfigurationChangeCallback();

        var section = configuration.GetSection("Server");
        var setting = section.Get<ServerSetting>();

        if (setting is null)
        {
            logger.LogError("failed to load server settings");
            return;
        }

        logger.LogInformation("reload banned players list: {Array}", string.Join(',', setting.BannedPlayers));
        session.BanPlayers(setting.BannedPlayers);
    }
}