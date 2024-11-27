using WFDS.Common.Steam;

namespace WFDS.Server.Core.Network;

internal class PacketSendService(SessionManager sessionManager) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var session in sessionManager.GetSessions())
            {
                // skip banned players
                if (sessionManager.IsBannedPlayer(session.SteamId))
                {
                    continue;
                }
                
                session.ProcessPacket();
            }
            
            await Task.Delay(1000 / 240, stoppingToken); // 240 fps
        }
    }
}