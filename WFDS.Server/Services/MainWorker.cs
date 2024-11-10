using Microsoft.Extensions.Options;
using Steamworks;
using WFDS.Server.Managers;

namespace WFDS.Server.Services;

public class MainWorker(
    ILogger<MainWorker> logger,
    IOptions<ServerSetting> settings,
    LobbyManager lobby,
    MapManager map
) : BackgroundService
{
    private const uint AppId = 3146520;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SteamClientService start");
        SteamClient.Init(AppId);
        SteamNetworking.AllowP2PPacketRelay(true);

        map.LoadSpawnPoints();

        lobby.CreateLobby(
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

        lobby.SelectSessions(session =>
        {
            session.ServerClose();
        });
        
        await lobby.LeaveLobbyAsync();
        SteamClient.Shutdown();
        logger.LogInformation("SteamClientService stopped");
    }
}