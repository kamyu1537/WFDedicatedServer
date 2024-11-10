using System.Collections.Concurrent;
using Steamworks;
using Steamworks.Data;
using WFDS.Common.Types;
using WFDS.Godot.Binary;
using WFDS.Server.Common;
using WFDS.Server.Common.Helpers;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Managers;

public sealed class LobbyManager : IDisposable
{
    private const int MaxPlayers = 16;
    private const string LobbyMode = "GodotsteamLobby";
    private const string LobbyRef = "webfishing_gamelobby";
    private const string GameVersion = "1.09";

    private readonly ILogger<LobbyManager> _logger;
    private readonly ILoggerFactory _loggerFactory;

    private readonly HashSet<ulong> _banned = [];
    private readonly ConcurrentDictionary<ulong, Session> _sessions = [];

    private bool _created;
    private string _name = string.Empty;
    private GameLobbyType _lobbyType = GameLobbyType.Public;
    private int _cap;
    private bool _public;
    private bool _adult;

    private Lobby? _lobby;
    public string Code { get; private set; } = string.Empty;

    public LobbyManager(ILogger<LobbyManager> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyMemberDataChanged += OnLobbyMemberDataChanged;
        SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequest;
    }

    public void Dispose()
    {
        if (_lobby.HasValue)
        {
            _lobby = null;
            SetPublic(false);
        }

        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyMemberDisconnected -= OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyMemberDataChanged -= OnLobbyMemberDataChanged;
        SteamNetworking.OnP2PSessionRequest -= OnP2PSessionRequest;

        _banned.Clear();
        _sessions.Clear();
    }

    public void CreateLobby(string serverName, string code, GameLobbyType lobbyType, bool @public, bool adult, int cap = MaxPlayers)
    {
        if (_created)
        {
            return;
        }

        Code = code;
        if (Code.Length < 5 || Code.Length > 6 || string.IsNullOrWhiteSpace(Code))
        {
            Code = GenerationRoomCode();
        }

        _name = serverName;
        _public = @public;
        _lobbyType = lobbyType;
        _adult = adult;
        _cap = cap;
        _created = true;

        SteamMatchmaking.CreateLobbyAsync(_cap);
    }

    private void SetupLobby(Lobby lobby)
    {
        lobby.SetData("mode", LobbyMode);
        lobby.SetData("ref", LobbyRef);
        lobby.SetData("version", GameVersion);
        lobby.SetData("server_browser_value", "0");


        lobby.SetData("name", _name);
        lobby.SetData("lobby_name", _name);
        lobby.SetData("cap", _cap.ToString());
        lobby.SetData("age_limit", _adult ? "true" : "");

        lobby.SetData("banned_players", "");

        SetLobbyType(_lobbyType);
        SetPublic(_public);

        UpdateBannedPlayers();
        UpdateConsoleTitle();
    }

    public void SetPublic(bool @public)
    {
        if (!_lobby.HasValue)
        {
            return;
        }

        _lobby.Value.SetJoinable(@public);
    }

    public void SetLobbyType(GameLobbyType type)
    {
        if (!_lobby.HasValue)
        {
            return;
        }

        var lobbyType = type switch
        {
            GameLobbyType.Public => "public",
            GameLobbyType.CodeOnly => "code_only",
            GameLobbyType.FriendsOnly => "friends_only",
            _ => "code_only"
        };

        _lobby.Value.SetData("type", lobbyType);
        _lobby.Value.SetData("public", _lobbyType == GameLobbyType.Public ? "true" : "false");

        if (_lobbyType is GameLobbyType.Public or GameLobbyType.CodeOnly)
        {
            _lobby.Value.SetData("code", Code);
        }
    }

    public async Task LeaveLobbyAsync()
    {
        if (_lobby.HasValue)
        {
            while (_sessions.Count > 0)
            {
                await Task.Delay(500);
            }
        }

        _lobby?.Leave();
    }

    public void ServerClose(SteamId target)
    {
        if (!_lobby.HasValue)
        {
            return;
        }

        if (target.Value == SteamClient.SteamId)
        {
            return;
        }

        SelectSession(target, session =>
        {
            _logger.LogInformation("try kick player: {SteamId}", target);
            session.SendPacket(NetChannel.GameState, new ServerClosePacket());
        });

        SteamNetworking.CloseP2PSessionWithUser(target);
    }

    public void KickPlayer(SteamId target)
    {
        if (!_lobby.HasValue)
        {
            return;
        }

        // 자기 자신은 킥을 하지 못한다.
        if (target.Value == SteamClient.SteamId)
        {
            return;
        }

        SelectSession(target, session =>
        {
            _logger.LogInformation("try kick player: {SteamId}", target);
            session.SendPacket(NetChannel.GameState, new KickPacket());
        });

        SteamNetworking.CloseP2PSessionWithUser(target);
    }

    public void TempBanPlayer(SteamId target, bool update = true)
    {
        if (!_banned.Add(target))
            return;

        SelectSession(target, session =>
        {
            _logger.LogInformation("try ban player: {SteamId}", target);
            session.SendPacket(NetChannel.GameState, new BanPacket());
        });

        SteamNetworking.CloseP2PSessionWithUser(target);

        if (update)
            UpdateBannedPlayers();
    }

    public void BanPlayers(string[] banPlayers)
    {
        foreach (var banPlayer in banPlayers)
        {
            if (ulong.TryParse(banPlayer, out var steamId))
            {
                TempBanPlayer(steamId, false);
            }
        }

        UpdateBannedPlayers();
    }

    public void RemoveBanPlayer(SteamId target)
    {
        if (!_banned.Remove(target))
        {
            return;
        }

        UpdateBannedPlayers();
    }

    private void UpdateBannedPlayers()
    {
        _lobby?.SetData("banned_players", string.Join(",", _banned));
    }

    public int GetSessionCount()
    {
        return _sessions.Count;
    }

    public bool IsSessionExists(SteamId steamId)
    {
        return _sessions.ContainsKey(steamId.Value);
    }

    public void SelectSessions(Action<Session> action)
    {
        foreach (var session in _sessions.Values)
        {
            action(session);
        }
    }

    public void SelectSession(SteamId target, Action<Session> action)
    {
        if (!_sessions.TryGetValue(target.Value, out var session))
        {
            return;
        }

        action(session);
    }

    private static bool IsInZone(Session session, string zone, long zoneOwner)
    {
        if (string.IsNullOrEmpty(zone))
        {
            return true;
        }

        if (session.Disposed)
        {
            return false;
        }

        if (!session.ActorCreated)
        {
            return false;
        }
        
        if (session.Actor == null)
        {
            return false;
        }

        var actorZone = session.Actor.Zone;
        var actorZoneOwner = session.Actor.ZoneOwner;

        return actorZone == zone && (zoneOwner == -1 || actorZoneOwner == zoneOwner);
    }

    public void SendPacket(SteamId steamId, NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1)
    {
        var data = packet.ToDictionary();
        SendPacket(steamId, channel, data, zone, zoneOwner);
    }

    public void SendPacket(SteamId steamId, NetChannel channel, object data, string zone = "", long zoneOwner = -1)
    {
        if (!_lobby.HasValue)
        {
            return;
        }

        if (!_sessions.TryGetValue(steamId.Value, out var session))
        {
            return;
        }

        if (!IsInZone(session, zone, zoneOwner))
        {
            return;
        }

        var bytes = GodotBinaryConverter.Serialize(data);
        var compressed = GZipHelper.Compress(bytes);
        session.Packets.Enqueue((channel, compressed));
    }

    public void BroadcastPacket(NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1)
    {
        var data = packet.ToDictionary();
        BroadcastPacket(channel, data, zone, zoneOwner);
    }

    public void BroadcastPacket(NetChannel channel, object data, string zone = "", long zoneOwner = -1)
    {
        if (!_lobby.HasValue)
        {
            return;
        }

        var bytes = GodotBinaryConverter.Serialize(data);
        var compressed = GZipHelper.Compress(bytes);

        foreach (var session in _sessions.Values)
        {
            if (session.SteamId.Value == SteamClient.SteamId.Value)
            {
                continue;
            }

            if (!IsInZone(session, zone, zoneOwner))
            {
                continue;
            }

            session.Packets.Enqueue((channel, compressed));
        }
    }

    private void RemoveSession(Friend member)
    {
        _logger.LogWarning("try remove session: {DisplayName} [{SteamId}]", member.Name, member.Id);
        SteamNetworking.CloseP2PSessionWithUser(member.Id);

        if (!_sessions.TryRemove(member.Id.Value, out var session))
        {
            _logger.LogWarning("failed to remove session: {DisplayName} [{SteamId}]", member.Name, member.Id);
            return;
        }

        session.Dispose();
        UpdateConsoleTitle();
    }

    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            _logger.LogError("failed to create lobby: {Result}", result);
            return;
        }

        _logger.LogInformation("lobby created: id({LobbyId}) roomCode({RoomCode}) lobbyType({LobbyType}) public({Public}) adult({Adult}) cap({Cap})", lobby.Id, Code, _lobbyType, _public, _adult, _cap);

        _lobby = lobby;
        SetupLobby(lobby);
        UpdateBrowserValue();
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend member)
    {
        _logger.LogInformation("lobby member joined: {DisplayName} [{SteamId}]", member.Name, member.Id);

        var logger = _loggerFactory.CreateLogger("session_" + member.Id);
        var session = new Session
        {
            LobbyManager = this,
            Logger = logger,
            Friend = member,
            SteamId = member.Id,
            ConnectTime = DateTimeOffset.UtcNow
        };

        if (!_sessions.TryAdd(member.Id.Value, session))
        {
            _logger.LogWarning("failed to add session: {DisplayName} [{SteamId}]", member.Name, member.Id);
            session.Kick();
            return;
        }

        UpdateConsoleTitle();
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend member)
    {
        SteamNetworking.CloseP2PSessionWithUser(member.Id);

        _logger.LogInformation("lobby member left: {DisplayName} [{SteamId}]", member.Name, member.Id);
        RemoveSession(member);
        UpdateConsoleTitle();
    }

    private void OnLobbyMemberDisconnected(Lobby lobby, Friend member)
    {
        SteamNetworking.CloseP2PSessionWithUser(member.Id);

        _logger.LogWarning("lobby member disconnected: {DisplayName} [{SteamId}]", member.Name, member.Id);
        RemoveSession(member);
        UpdateConsoleTitle();
    }

    private void OnLobbyMemberDataChanged(Lobby lobby, Friend member)
    {
        _logger.LogInformation("lobby member data changed: {DisplayName} [{SteamId}]", member.Name, member.Id);

        SelectSession(member.Id, session => { session.Friend = member; });
    }

    private void OnP2PSessionRequest(SteamId requester)
    {
        _logger.LogWarning("P2P session request: {SteamId}", requester);
        SelectSession(requester, _ => { SteamNetworking.AcceptP2PSessionWithUser(requester); });
    }

    private static string GenerationRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var code = new char[6];
        var random = new Random();
        for (var i = 0; i < code.Length; i++)
        {
            code[i] = chars[random.Next(chars.Length)];
        }

        return new string(code);
    }

    public void UpdateBrowserValue()
    {
        if (!_lobby.HasValue)
        {
            return;
        }

        var random = new Random();

        _lobby.Value.SetData("server_browser_value", (random.Next() % 20).ToString());
    }

    public void KickNoHandshakePlayers()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var session in _sessions.Values)
        {
            if (!session.HandshakeReceived && now - session.ConnectTime > TimeSpan.FromMinutes(5))
            {
                KickPlayer(session.SteamId);
            }
        }
    }

    private void UpdateConsoleTitle()
    {
        Console.Title = $"[{_sessions.Count}/{_cap}] {_name} [{Code}]";
    }
}