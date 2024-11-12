using Microsoft.Extensions.Options;
using Steamworks;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.Services;

public class WFServer(
    ILogger<WFServer> logger,
    IOptions<ServerSetting> settings,
    ISessionManager session,
    IMapManager map
) : BackgroundService
{
    private const uint AppId = 3146520;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SteamClientService start");
        SteamClient.Init(AppId);
        SteamNetworking.AllowP2PPacketRelay(true);

        map.LoadSpawnPoints();

        session.CreateLobby(
            settings.Value.ServerName,
            settings.Value.RoomCode,
            settings.Value.LobbyType,
            settings.Value.Public,
            settings.Value.Adult,
            settings.Value.MaxPlayers
        );
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        session.SelectSessions(player => {
            player.ServerClose();
        });
        
        await session.LeaveLobbyAsync();
        SteamClient.Shutdown();
        
        logger.LogInformation("MainWorker stopped");
        await base.StopAsync(cancellationToken);
    }
}