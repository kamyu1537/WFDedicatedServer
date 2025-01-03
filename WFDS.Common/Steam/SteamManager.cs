using Microsoft.Extensions.Logging;
using Steamworks;
using WFDS.Common.Types;


namespace WFDS.Common.Steam;

public class SteamManager : Singleton<SteamManager>
{
    public bool Initialized { get; private set; }
    public CSteamID SteamId => Initialized ? _steamId : CSteamID.Nil;
    public string PersonalName => Initialized ? _personalName : string.Empty;
    
    private readonly ILogger _logger = Log.Factory.CreateLogger<SteamManager>();
    private CSteamID _steamId;
    private string _personalName = string.Empty;

    public bool Init()
    {
        if (Initialized) return true;
        Environment.SetEnvironmentVariable("SteamAppId", "3146520");
        var result = SteamAPI.InitEx(out var errMsg);
        if (result != ESteamAPIInitResult.k_ESteamAPIInitResult_OK)
        {
            _logger.LogError("SteamAPI.Init failed: {Result} {ErrorMessage}", result, errMsg);
            return false;
        }
        
        Initialized = true;
        _steamId = SteamUser.GetSteamID();
        _personalName = SteamFriends.GetPersonaName();
        
        _logger.LogInformation("SteamAPI.Init success");
        return true;
    }

    public void Shutdown()
    {
        Initialized = false;
        _steamId = CSteamID.Nil;
        _personalName = string.Empty;
        
        SteamAPI.Shutdown();
    }
    
    public void RunCallbacks()
    {
        if (!Initialized) return;
        SteamAPI.RunCallbacks();
    }
}