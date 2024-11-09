using FSDS.Server.Common;
using FSDS.Server.Managers;
using Steamworks;

namespace FSDS.Server.Services;

public class ServerInitializeService(
    ILogger<ServerInitializeService> logger,
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
        lobby.CreateLobby("korea server - dev", GameLobbyType.CodeOnly, true, false);

        while (!stoppingToken.IsCancellationRequested)
        {
            SteamClient.RunCallbacks();
            await Task.Delay(1000 / 10, stoppingToken); // 10 fps
        }


        SteamClient.Shutdown();
        logger.LogInformation("SteamClientService stopped");
    }
}