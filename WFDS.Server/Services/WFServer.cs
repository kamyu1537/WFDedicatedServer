using Microsoft.Extensions.Options;
using Steamworks;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.Services;

public class WFServer(
    ILogger<WFServer> logger,
    IOptions<ServerSetting> settings,
    IGameSessionManager session,
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
            if (session.IsLobbyValid())
            {
                session.CreateLobby(
                    settings.Value.ServerName,
                    settings.Value.RoomCode,
                    settings.Value.LobbyType,
                    settings.Value.Public,
                    settings.Value.Adult,
                    settings.Value.MaxPlayers
                );
            }
            
            await Task.Delay(1000, stoppingToken);
        }

        await Cleanup();
    }

    private async Task Cleanup()
    {
        var sessions = session.GetSessions();
        foreach (var player in sessions)
        {
            player.ServerClose();
        }
        
        await session.LeaveLobbyAsync();
        SteamClient.Shutdown();
    }
}