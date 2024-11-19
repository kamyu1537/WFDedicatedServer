using WFDS.Common.Actor;
using WFDS.Common.Types.Manager;
using WFDS.Server.Core.Network;
using WFDS.Server.Core.Steam;

namespace WFDS.Server.Core.Actor;

internal sealed class AmbientSpawnScheduleService(IActorSpawnManager spawn, SteamManager steam) : IHostedService
{
    private static readonly TimeSpan AmbientSpawnTimeoutPeriod = TimeSpan.FromSeconds(10);
    private readonly Random _random = new();
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, AmbientSpawnTimeoutPeriod);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_timer != null)
            await _timer.DisposeAsync();
    }

    // _on_ambient_spawn_timer_timeout
    private void DoWork(object? state)
    {
        if (!steam.Initialized)
        {
            return;
        }
        
        var index = _random.Next() % 3;

        if (index == 2)
            spawn.SpawnAmbientBirdActor();
    }
}