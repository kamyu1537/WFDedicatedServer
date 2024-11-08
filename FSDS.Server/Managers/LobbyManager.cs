using System.Collections.Concurrent;
using System.Text.Json;
using FSDS.Godot.Binary;
using FSDS.Server.Common;
using FSDS.Server.Common.Helpers;
using FSDS.Server.Packets;
using Steamworks;
using Steamworks.Data;

namespace FSDS.Server.Managers;

public sealed class LobbyManager : IDisposable
{
    private const int MaxPlayers = 16;
    private const string LobbyMode = "GodotsteamLobby";
    private const string LobbyRef = "webfishing_gamelobby";
    private const string GameVersion = "1.09";

    private readonly ILogger<LobbyManager> _logger;
    
    private readonly HashSet<ulong> _banned = [];
    private readonly ConcurrentDictionary<ulong, Session> _sessions = [];
    private readonly ConcurrentQueue<(SteamId, NetChannel, byte[])> _packets = [];

    private bool _created;
    private string _name = string.Empty;
    private string _code = string.Empty;
    private GameLobbyType _lobbyType = GameLobbyType.Public;
    private int _cap;
    private bool _public;
    private bool _adult;

    private Lobby? _lobby;

    public LobbyManager(ILogger<LobbyManager> logger)
    {
        _logger = logger;

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
        SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequest;
    }

    public void Dispose()
    {
        if (_lobby.HasValue)
        {
            var lobby = _lobby.Value;
            _lobby = null;
            SetPublic(false);

            BroadcastPacket(NetChannel.GameState, new ServerClosePacket());
            lobby.Leave();
        }

        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyMemberDisconnected -= OnLobbyMemberDisconnected;
        SteamNetworking.OnP2PSessionRequest -= OnP2PSessionRequest;
        
        _banned.Clear();
        _sessions.Clear();
    }
    
    public void CreateLobby(string name, GameLobbyType lobbyType, bool @public, bool adult, int cap = MaxPlayers)
    {
        if (_created)
        {
            return;
        }

        // _code = GenerationRoomCode();
        _code = "KAMYU";
        _name = name;
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
        lobby.SetData("server_browser_value", "7");


        lobby.SetData("name", _name);
        lobby.SetData("lobby_name", _name);
        
        lobby.SetData("code", _code);
        lobby.SetData("cap", _cap.ToString());

        lobby.SetData("banned_players", "");
        lobby.SetData("age_limit", _adult ? "true" : "");

        SetLobbyType(_lobbyType);
        SetPublic(_public);
        
        UpdateBannedPlayers();
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

        _lobby.Value.SetData("type",lobbyType);
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

        SessionExecute(target, session =>
        {
            _logger.LogInformation("kicking player: {SteamId}", target);
            session.Send(NetChannel.GameState, new KickPacket());
            _sessions.TryRemove(target.Value, out _);
        });
    }

    public void BanPlayer(ulong steamId)
    {
        if (!_banned.Add(steamId))
        {
            return;
        }

        UpdateBannedPlayers();
    }

    public void RemoveBanPlayer(ulong steamId)
    {
        if (!_banned.Remove(steamId))
        {
            return;
        }

        UpdateBannedPlayers();
    }

    private void UpdateBannedPlayers()
    {
        if (_lobby.HasValue)
        {
            _lobby.Value.SetData("banned_players", string.Join(",", _banned));   
        }
    }

    public bool IsSessionExists(SteamId steamId)
    {
        return _sessions.ContainsKey(steamId.Value);
    }
    
    public void SessionForEach(Action<Session> action)
    {
        foreach (var session in _sessions.Values)
        {
            action(session);
        }
    }
    
    public bool SessionExecute(SteamId target, Action<Session> action)
    {
        if (!_sessions.TryGetValue(target.Value, out var session))
        {
            return false;
        }

        action(session);
        return true;
    }

    public void SendPacket(SteamId steamId, NetChannel channel, IPacket packet)
    {
        var data = packet.ToDictionary();
        SendPacket(steamId, channel, data);
    }
    
    public void SendPacket(SteamId steamId, NetChannel channel, object data)
    {
        if (!_lobby.HasValue)
        {
            return;
        }
        
        if (!_sessions.ContainsKey(steamId.Value))
        {
            return;
        }
        
        var bytes = GodotBinaryConverter.Serialize(data);
        var compressed = GZipHelper.Compress(bytes);
        _packets.Enqueue((steamId, channel, compressed));
        // SteamNetworking.SendP2PPacket(steamId, compressed, nChannel: channel.Value);
    }
    
    public void BroadcastPacket(NetChannel channel, IPacket packet)
    {
        var data = packet.ToDictionary();
        BroadcastPacket(channel, data);
    }

    public void BroadcastPacket(NetChannel channel, object data)
    {
        if (!_lobby.HasValue)
        {
            return;
        }
        
        var bytes = GodotBinaryConverter.Serialize(data);
        var compressed = GZipHelper.Compress(bytes);
        
        foreach (var id in _sessions.Values.Select(x => x.SteamId))
        {
            if (id.Value == SteamClient.SteamId.Value)
            {
                continue;
            }
            
            _packets.Enqueue((id, channel, compressed));
            // SteamNetworking.SendP2PPacket(id, compressed, nChannel: channel.Value);
        }
    }
    
    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            _logger.LogError("failed to create lobby: {Result}", result);
            return;
        }

        _logger.LogInformation("lobby created: {LobbyId} [{RoomCode}]", lobby.Id, _code);
        
        _lobby = lobby;
        SetupLobby(lobby);
        
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend member)
    {
        _logger.LogInformation("lobby member joined: {DisplayName} [{SteamId}]", member.Name, member.Id);

        var session = new Session
        {
            LobbyManager = this,
            Friend = member,
            SteamId = member.Id,
            ConnectTime = DateTimeOffset.UtcNow
        };
        
        _sessions.TryAdd(member.Id.Value, session);
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend member)
    {
        _logger.LogInformation("lobby member left: {DisplayName} [{SteamId}]", member.Name, member.Id);
        _sessions.TryRemove(member.Id.Value, out _);
    }

    private void OnLobbyMemberDisconnected(Lobby lobby, Friend member)
    {
        _logger.LogWarning("lobby member disconnected: {DisplayName} [{SteamId}]", member.Name, member.Id);
        _sessions.TryRemove(member.Id.Value, out _);
    }

    private void OnP2PSessionRequest(SteamId requester)
    {
        _logger.LogWarning("P2P session request: {SteamId}", requester);
        SessionExecute(requester, _ => { SteamNetworking.AcceptP2PSessionWithUser(requester); });
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
    
    public void ProcessPackets()
    {
        var count = 50;
        while (_packets.TryDequeue(out var packet))
        {
            if (count-- <= 0)
            {
                break;
            }

            // Console.WriteLine(JsonSerializer.Serialize(GodotBinaryConverter.Deserialize(GZipHelper.Decompress(packet.Item3))));
            SteamNetworking.SendP2PPacket(packet.Item1, packet.Item3, nChannel: packet.Item2.Value);
        }
    }
}