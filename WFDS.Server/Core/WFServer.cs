using Microsoft.Extensions.Options;
using Steamworks;
using WFDS.Common.Types.Manager;
using WFDS.Server.Core.Configuration;

namespace WFDS.Server.Core;

internal class WFServer(
    IHostApplicationLifetime lifetime,
    ILogger<WFServer> logger,
    IOptions<ServerSetting> settings,
    ISessionManager session,
    IZoneManager zone
) : BackgroundService
{
    private const uint AppId = 3146520;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SteamClientService start");
        SteamClient.Init(AppId);
        SteamNetworking.AllowP2PPacketRelay(true);

        zone.LoadZones();

        session.BanPlayers(settings.Value.BannedPlayers);
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
            
            if (!session.IsLobbyValid())
            {
                logger.LogWarning("Lobby is not valid, shutting down");
                break;
            }
        }
        
        // 프로그램을 를 종료한다.
        logger.LogInformation("SteamClientService stop");
        lifetime.StopApplication();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        session.ServerClose();
        SteamClient.Shutdown();
        
        await base.StopAsync(cancellationToken);
    }
}