using FSDS.Server.Managers;
using FSDS.Server.Services;

namespace FSDS.Server.Systems;

public class SessionManagementSystem(LobbyManager lobbyManager)
{
    private const double Timeout = 30;
    private const double UpdateInterval = 1.0;
    private double _time;
    
    public void Update(double delta)
    {
        _time += delta;
        if (_time < UpdateInterval)
        {
            return;
        }
        _time = 0;
        
        lobbyManager.SessionForEach(session =>
        {
            if (session.HandshakeReceived)
            {
                return;
            }
            
            if (session.ConnectTime.AddSeconds(Timeout) < DateTimeOffset.UtcNow)
            {
                lobbyManager.KickPlayer(session.SteamId);
            }
        });
    }
}