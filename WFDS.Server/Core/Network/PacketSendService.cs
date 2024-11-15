using WFDS.Common.Types.Manager;

namespace WFDS.Server.Core.Network;

internal class PacketSendService(ISessionManager sessionManager) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var session in sessionManager.GetSessions())
            {
                session.ProcessPacket();
            }
            
            await Task.Delay(1000 / 240, stoppingToken); // 240 fps
        }
    }
}