using Microsoft.Extensions.Logging;
using Steamworks;
using WFDS.Common.Types;
using ZLogger;

namespace WFDS.Common.Steam;

public class SteamManager : Singleton<SteamManager>
{
    public bool Initialized { get; private set; }
    public CSteamID SteamId => Initialized ? _steamId : CSteamID.Nil;
    
    private readonly ILogger _logger = Log.Factory.CreateLogger<SteamManager>();
    private CSteamID _steamId;

    public bool Init()
    {
        if (Initialized) return true;
        Environment.SetEnvironmentVariable("SteamAppId", "3146520");
        var result = SteamAPI.InitEx(out var errMsg);
        if (result != ESteamAPIInitResult.k_ESteamAPIInitResult_OK)
        {
            _logger.ZLogError($"SteamAPI.Init failed: {result} {errMsg}");
            return false;
        }
        
        Initialized = true;
        _steamId = SteamUser.GetSteamID();
        _logger.ZLogInformation($"SteamAPI.Init success");
        return true;
    }

    public void Shutdown()
    {
        SteamAPI.Shutdown();
        Initialized = false;
        _steamId = CSteamID.Nil;
    }
    
    public void RunCallbacks()
    {
        SteamAPI.RunCallbacks();
    }
}