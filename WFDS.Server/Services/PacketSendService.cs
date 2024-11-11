using WFDS.Common.Types.Manager;

namespace WFDS.Server.Services;

public class PacketSendService(ISessionManager session) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            session.SelectSessions(player => player.ProcessPacket());
            await Task.Delay(1000 / 240, stoppingToken); // 240 fps
        }
    }
}