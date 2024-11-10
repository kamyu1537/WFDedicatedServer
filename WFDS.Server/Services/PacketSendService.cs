using WFDS.Server.Managers;

namespace WFDS.Server.Services;

public class PacketSendService(LobbyManager lobby) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            lobby.SelectSessions(session => session.ProcessPacket());
            await Task.Delay(1000 / 240, stoppingToken); // 240 fps
        }
    }
}