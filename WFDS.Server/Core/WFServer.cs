using Microsoft.Extensions.Options;
using Steamworks;
using WFDS.Common.Types.Manager;
using WFDS.Server.Core.Configuration;

namespace WFDS.Server.Core;

internal class WFServer(
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
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        var sessions = session.GetSessions();
        foreach (var player in sessions)
        {
            session.ServerClose(player.SteamId);
        }
        
        await session.LeaveLobbyAsync();
        SteamClient.Shutdown();
        
        await base.StopAsync(cancellationToken);
    }
}