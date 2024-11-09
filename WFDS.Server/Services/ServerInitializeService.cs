using WFDS.Server.Common;
using WFDS.Server.Managers;
using Microsoft.Extensions.Options;
using Steamworks;

namespace WFDS.Server.Services;

public class ServerInitializeService(
    ILogger<ServerInitializeService> logger,
    IOptions<ServerSetting> settings,
    LobbyManager lobby,
    MapManager map
) : BackgroundService
{
    private const uint AppId = 3146520;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SteamClientService start");
        SteamClient.Init(AppId, false);
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
            SteamClient.RunCallbacks();
            await Task.Delay(1000 / 10, stoppingToken); // 10 fps
        }


        SteamClient.Shutdown();
        logger.LogInformation("SteamClientService stopped");
    }
}