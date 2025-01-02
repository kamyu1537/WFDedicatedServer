using Cysharp.Threading;
using Microsoft.Extensions.Options;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using WFDS.Server.Core.Configuration;


namespace WFDS.Server.Core;

internal class WebFishingServer(
    IHostApplicationLifetime lifetime,
    ILogger<WebFishingServer> logger,
    IOptions<ServerSetting> settings,
    SteamManager steam,
    LobbyManager lobby,
    SessionManager session,
    ICanvasManager canvas
) : BackgroundService
{
    private readonly Mutex _mutex = new(false, "WFDS.Server");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_mutex.WaitOne(0, false))
        {
            logger.LogError("WFDS.Server is already running");
            lifetime.StopApplication();
            return;
        }

        logger.LogInformation("WebFishingServer start");
        _ = LogicLooperPool.Shared.RegisterActionAsync(Update).ConfigureAwait(false);

        if (!steam.Init())
        {
            lifetime.StopApplication();
            return;
        }

        canvas.LoadAll();
        lobby.Initialize(
            settings.Value.ServerName,
            settings.Value.LobbyType, 
            settings.Value.MaxPlayers, 
            settings.Value.Adult,
            settings.Value.RoomCode,
            settings.Value.LobbyTags.Select(LobbyTag.Get).ToArray()
        );
        lobby.CreateLobby();

        while (!stoppingToken.IsCancellationRequested)
        {
            if (lobby.GetLobbyId().IsValid() && !lobby.IsInLobby())
            {
                break;
            }

            canvas.SaveChanges();
            await Task.Delay(1000, stoppingToken);
        }

        // 프로그램을 를 종료한다.
        lobby.LeaveLobby(out _);
        logger.LogInformation("WebFishingServer stop");
        lifetime.StopApplication();
    }

    private bool Update(in LogicLooperActionContext ctx)
    {
        if (ctx.CancellationToken.IsCancellationRequested)
        {
            logger.LogError("update canceled");
            return false;
        }

        if (!steam.Initialized)
        {
            return true;
        }

        steam.RunCallbacks();
        return true;
    }

    private void ServerClose()
    {
        logger.LogInformation("try session close");
        session.ServerClose();
    }

    private void Shutdown()
    {
        logger.LogInformation("steam api shutdown");
        steam.Shutdown();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        ServerClose();
        Shutdown();

        logger.LogInformation("try steam looper is stopping");
        await base.StopAsync(cancellationToken);

        logger.LogInformation("WebFishingServer stopped");
    }
}