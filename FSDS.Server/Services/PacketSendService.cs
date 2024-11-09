using FSDS.Server.Managers;

namespace FSDS.Server.Services;

public class PacketSendService(LobbyManager lobby) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            lobby.SelectSessions(session => session.ProcessPackets());
            await Task.Delay(1000 / 15, stoppingToken);
        }
    }
}