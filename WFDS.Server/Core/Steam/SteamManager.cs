using Steamworks;

namespace WFDS.Server.Core.Steam;

internal class SteamManager(ILogger<SteamManager> logger)
{
    public bool Initialized { get; private set; }
    public CSteamID SteamId { get; private set; }

    public bool Init()
    {
        if (Initialized) return true;
        
        Environment.SetEnvironmentVariable("SteamAppId", "3146520");
        var result = SteamAPI.InitEx(out var errMsg);
        if (result != ESteamAPIInitResult.k_ESteamAPIInitResult_OK)
        {
            logger.LogError("SteamAPI.Init failed: {Result} {Error}", result, errMsg);
            return false;
        }
        
        Initialized = true;
        SteamId = SteamUser.GetSteamID();
        logger.LogInformation("SteamAPI.Init success");
        return true;
    }
}