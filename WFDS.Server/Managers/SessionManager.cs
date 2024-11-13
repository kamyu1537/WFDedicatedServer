using System.Collections.Concurrent;
using Steamworks;
using Steamworks.Data;
using WFDS.Common.Helpers;
using WFDS.Common.Network;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Godot.Binary;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Managers;

public sealed class SessionManager : ISessionManager
{
    private const int MaxPlayers = 16;
    private const int RoomCodeLength = 6;
    private const string LobbyMode = "GodotsteamLobby";
    private const string LobbyRef = "webfishing_gamelobby";
    private const string GameVersion = "1.1";

    private readonly ILogger<SessionManager> _logger;
    private readonly ILoggerFactory _loggerFactory;

    private readonly HashSet<ulong> _banned = [];
    private readonly ConcurrentDictionary<ulong, Session> _sessions = [];

    private bool _created;
    private string _name = string.Empty;
    private GameLobbyType _lobbyType = GameLobbyType.Public;
    private int _cap;
    private bool _public;
    private bool _adult;
    private string _code = string.Empty;

    private Lobby? _lobby;

    public SessionManager(ILogger<SessionManager> logger, ILoggerFactory loggerFactory)
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

    public void CreateLobby(string serverName, string code, GameLobbyType lobbyType, bool @public, bool adult, int cap = MaxPlayers)
    {
        if (_created)
        {
            return;
        }

        _code = code;
        if (_code.Length != RoomCodeLength || string.IsNullOrWhiteSpace(_code))
        {
            _code = GenerationRoomCode();
        }

        _name = serverName;
        _public = @public;
        _lobbyType = lobbyType;
        _adult = adult;
        _cap = cap;
        _created = true;

        SteamMatchmaking.CreateLobbyAsync(_cap + 1);
    }

    private void SetupLobby(Lobby lobby)
    {
        lobby.SetData("mode", LobbyMode);
        lobby.SetData("ref", LobbyRef);
        lobby.SetData("version", GameVersion);
        lobby.SetData("server_browser_value", "0");


        lobby.SetData("name", _name);
        lobby.SetData("lobby_name", _name);
        lobby.SetData("cap", (_cap + 1).ToString());
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
            _lobby.Value.SetData("code", _code);
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

    private void UpdateBannedPlayers()
    {
        _lobby?.SetData("banned_players", string.Join(",", _banned));
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

    private void RemoveSession(Friend member)
    {
        _logger.LogWarning("try remove session: {Member}", member);
        SteamNetworking.CloseP2PSessionWithUser(member.Id);

        if (!_sessions.TryRemove(member.Id.Value, out var session))
        {
            _logger.LogWarning("failed to remove session: {Member}", member);
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

        _logger.LogInformation("lobby created: id({LobbyId}) roomCode({RoomCode}) lobbyType({LobbyType}) public({Public}) adult({Adult}) cap({Cap})", lobby.Id, _code, _lobbyType, _public, _adult, _cap);

        _lobby = lobby;
        SetupLobby(lobby);
        UpdateBrowserValue();
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend member)
    {
        _logger.LogInformation("lobby member joined: {Member}", member);

        var logger = _loggerFactory.CreateLogger("session_" + member.Id);
        var session = new Session(this, logger)
        {
            Friend = member,
            SteamId = member.Id,
            ConnectTime = DateTimeOffset.UtcNow
        };

        if (!_sessions.TryAdd(member.Id.Value, session))
        {
            _logger.LogWarning("failed to add session: {Member}", member);
            session.Kick();
            return;
        }

        UpdateConsoleTitle();
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend member)
    {
        SteamNetworking.CloseP2PSessionWithUser(member.Id);

        _logger.LogInformation("lobby member left: {Member}", member);
        RemoveSession(member);
    }

    private void OnLobbyMemberDisconnected(Lobby lobby, Friend member)
    {
        SteamNetworking.CloseP2PSessionWithUser(member.Id);

        _logger.LogWarning("lobby member disconnected: {Member}", member);
        RemoveSession(member);
    }

    private void OnLobbyMemberDataChanged(Lobby lobby, Friend member)
    {
        _logger.LogInformation("lobby member data changed: {Member}", member);

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
        var code = new char[RoomCodeLength];
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
                _logger.LogError("kick no handshake player: {Member}", session.Friend);
                KickPlayer(session.SteamId);
            }
        }
    }

    private void UpdateConsoleTitle()
    {
        var newTitle = $"[{_sessions.Count}/{_cap}] {_name} [{_code}]";
        _logger.LogInformation("update console title: {Title}", newTitle);
        Console.Title = newTitle;
    }

    // ------------------- Interface Implementation -------------------
    public int GetSessionCount()
    {
        return _sessions.Count;
    }

    public bool IsSessionValid(SteamId steamId)
    {
        return _sessions.ContainsKey(steamId.Value);
    }

    public void SelectSessions(Action<ISession> action)
    {
        foreach (var session in _sessions.Values)
        {
            action(session);
        }
    }

    public void SelectSession(SteamId target, Action<ISession> action)
    {
        if (!_sessions.TryGetValue(target.Value, out var session))
        {
            return;
        }

        action(session);
    }

    public void SendP2PPacket(SteamId steamId, NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1)
    {
        var data = PacketHelper.ToDictionary(packet);
        SendP2PPacket(steamId, channel, data, zone, zoneOwner);
    }

    public void SendP2PPacket(SteamId steamId, NetChannel channel, object data, string zone = "", long zoneOwner = -1)
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
    
    public void BroadcastP2PPacket(NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1)
    {
        var data = PacketHelper.ToDictionary(packet);
        BroadcastP2PPacket(channel, data, zone, zoneOwner);
    }

    public void BroadcastP2PPacket(NetChannel channel, object data, string zone = "", long zoneOwner = -1)
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
                // _logger.LogDebug("skip broadcast to {Member} - {Zone}/{ZoneOwner}", session.Friend, session.Actor?.Zone ?? "unknown", session.Actor?.ZoneOwner ?? 0);
                continue;
            }

            session.Packets.Enqueue((channel, compressed));
        }
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
            _logger.LogInformation("try kick player: {Member}", session.Friend);
            session.SendP2PPacket(NetChannel.GameState, new ServerClosePacket());
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
            _logger.LogInformation("try kick player: {Member}", session.Friend);
            session.SendP2PPacket(NetChannel.GameState, new KickPacket());
        });

        SteamNetworking.CloseP2PSessionWithUser(target);
    }

    public void TempBanPlayer(SteamId target, bool update = true)
    {
        if (!_banned.Add(target))
            return;

        SelectSession(target, session =>
        {
            _logger.LogInformation("try ban player: {Member}", session.Friend);
            session.SendP2PPacket(NetChannel.GameState, new BanPacket());
            BroadcastP2PPacket(NetChannel.GameState, new ForceDisconnectPlayerPacket { UserId = session.SteamId });
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
}