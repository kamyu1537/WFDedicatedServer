using WFDS.Common.Actor;
using WFDS.Common.Steam;
using ZLogger;

namespace WFDS.Server.Core.Actor;

internal sealed class MetalSpawnScheduleService(ILogger<MetalSpawnScheduleService> logger, IActorSpawnManager spawn, SteamManager steam) : IHostedService
{
    private static readonly TimeSpan MetalSpawnTimeoutPeriod = TimeSpan.FromSeconds(20);
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, MetalSpawnTimeoutPeriod);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_timer != null)
            await _timer.DisposeAsync();
    }

    private void DoWork(object? state)
    {
        if (!steam.Initialized)
        {
            return;
        }
        
        var metal = spawn.SpawnMetalActor();
        if (metal != null)
        {
            logger.ZLogInformation($"spawn {metal.Type} ({metal.ActorId}) at {metal.Position}");
        }
    }

    
}