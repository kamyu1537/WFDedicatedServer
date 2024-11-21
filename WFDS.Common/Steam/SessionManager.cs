using System.Collections.Concurrent;
using System.Globalization;
using Serilog;
using Steamworks;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Helpers;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using WFDS.Godot.Binary;

namespace WFDS.Server.Core.Network;

public sealed class SessionManager : Singleton<SessionManager>, IDisposable
{
    private readonly ILogger _logger;

    private bool _closed;
    private readonly HashSet<string> _banned = [];
    private readonly ConcurrentDictionary<ulong, Session> _sessions = [];

    private readonly Callback<LobbyChatUpdate_t> _lobbyChatUpdateCallback;
    private readonly Callback<P2PSessionRequest_t> _p2pSessionRequestCallback;

    public SessionManager()
    {
        _logger = Log.ForContext<SessionManager>();
        _lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        _p2pSessionRequestCallback = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
    }

    public void Dispose()
    {
        ServerClose();

        _lobbyChatUpdateCallback.Dispose();
        _p2pSessionRequestCallback.Dispose();
    }


    public bool IsServerClosed()
    {
        return _closed;
    }

    public void ServerClose()
    {
        if (_closed)
        {
            return;
        }

        _closed = true;
        var sessions = GetSessions();
        foreach (var player in sessions)
        {
            ServerClose(player.SteamId);
        }
    }

    public void ServerClose(CSteamID target)
    {
        if (target == SteamManager.Inst.SteamId)
        {
            return;
        }

        _logger.Information("try server close player: {SteamId}", target);
        SendP2PPacket(target, NetChannel.GameState, new ServerClosePacket(), false);
    }


    public int GetSessionCount()
    {
        return _sessions.Count;
    }

    public Session? GetSession(CSteamID steamId)
    {
        return _sessions.TryGetValue(steamId.m_SteamID, out var session) ? session : null;
    }

    public IEnumerable<Session> GetSessions()
    {
        return _sessions.Values;
    }

    public bool IsSessionValid(CSteamID steamId)
    {
        return _sessions.ContainsKey(steamId.m_SteamID);
    }

    private bool TryCreateSession(CSteamID steamId)
    {
        if (_sessions.TryGetValue(steamId.m_SteamID, out _))
        {
            _logger.Warning("session already exists: {Member}", steamId);
            return false;
        }

        if (_banned.Contains(steamId.m_SteamID.ToString(CultureInfo.InvariantCulture)))
        {
            _logger.Warning("banned player: {Member}", steamId);
            return false;
        }

        _logger.Information("try create session: {Member}", steamId);
        var session = new Session(steamId);
        if (_sessions.TryAdd(steamId.m_SteamID, session))
        {
            return true;
        }

        _logger.Warning("failed to create session: {Member}", steamId);
        return false;
    }

    private void RemoveSession(CSteamID steamId)
    {
        _logger.Warning("try remove session: {Member}", steamId);
        SteamNetworking.CloseP2PSessionWithUser(steamId);

        if (!_sessions.TryRemove(steamId.m_SteamID, out _))
        {
            _logger.Warning("failed to remove session: {Member}", steamId);
            return;
        }

        GameEventBus.Publish(new PlayerLeaveEvent(steamId));
    }


    public void KickPlayer(CSteamID target)
    {
        if (target == SteamManager.Inst.SteamId)
        {
            return;
        }

        _logger.Information("try kick player: {Member}", target);
        SendP2PPacket(target, NetChannel.GameState, new KickPacket(), false);
    }


    public bool IsBannedPlayer(CSteamID target)
    {
        return _banned.Contains(target.m_SteamID.ToString(CultureInfo.InvariantCulture));
    }

    public void TempBanPlayer(CSteamID lobbyId, CSteamID target)
    {
        _logger.Information("try ban player: {Member}", target);
        SendP2PPacket(target, NetChannel.GameState, new BanPacket(), false);
        BroadcastP2PPacket(lobbyId, NetChannel.GameState, new ForceDisconnectPlayerPacket { UserId = target }, false);

        _banned.Add(target.m_SteamID.ToString(CultureInfo.InvariantCulture));
        GameEventBus.Publish(new PlayerBanEvent(target));
    }

    public void BanPlayers(CSteamID lobbyId, string[] banPlayers)
    {
        foreach (var banPlayer in banPlayers)
        {
            if (ulong.TryParse(banPlayer, NumberStyles.Any, CultureInfo.InvariantCulture, out var steamId))
            {
                TempBanPlayer(lobbyId, new CSteamID(steamId));
            }
        }
    }

    public void RemoveBanPlayer(CSteamID target)
    {
        if (!_banned.Remove(target.m_SteamID.ToString(CultureInfo.InvariantCulture)))
        {
            return;
        }

        GameEventBus.Publish(new PlayerUnBanEvent(target));
    }

    public IEnumerable<string> GetBannedPlayers()
    {
        return _banned.Select(x => x.ToString(CultureInfo.InvariantCulture));
    }


    public void SendP2PPacket(CSteamID steamId, NetChannel channel, Packet packet, bool useSession = true)
    {
        var data = PacketHelper.ToDictionary(packet);
        SendP2PPacket(steamId, channel, data, useSession);
    }

    private void SendP2PPacket(CSteamID steamId, NetChannel channel, object data, bool useSession = true)
    {
        if (!steamId.IsValid())
        {
            return;
        }

        if (useSession)
        {
            if (!_sessions.TryGetValue(steamId.m_SteamID, out var session))
            {
                return;
            }

            var bytes = GodotBinaryConverter.Serialize(data);
            var compressed = GZipHelper.Compress(bytes);
            session.Packets.Enqueue((channel, compressed));
        }
        else
        {
            var bytes = GodotBinaryConverter.Serialize(data);
            var compressed = GZipHelper.Compress(bytes);
            SteamNetworkingHelper.SendP2PPacket(steamId, channel, compressed);
        }
    }

    public void BroadcastP2PPacket(CSteamID lobbyId, NetChannel channel, Packet packet, bool useSession = true)
    {
        var data = PacketHelper.ToDictionary(packet);
        BroadcastP2PPacket(lobbyId, channel, data, useSession);
    }

    private void BroadcastP2PPacket(CSteamID lobbyId, NetChannel channel, object data, bool useSession = true)
    {
        if (!lobbyId.IsValid() || !lobbyId.IsLobby())
        {
            return;
        }

        if (useSession)
        {
            if (_sessions.Count > 0)
            {
                var bytes = GodotBinaryConverter.Serialize(data);
                var compressed = GZipHelper.Compress(bytes);

                foreach (var session in _sessions.Values)
                {
                    session.Packets.Enqueue((channel, compressed));
                }
            }
        }
        else
        {
            SteamNetworkingHelper.BroadcastP2PPacket(lobbyId, channel, data);
        }
    }

    /* ************************************************************************ */
    /* Steamworks Callbacks                                                     */
    /* ************************************************************************ */

    #region Steamworks Callbacks

    private void OnLobbyChatUpdate(LobbyChatUpdate_t param)
    {
        var lobbyId = new CSteamID(param.m_ulSteamIDLobby);
        var changedUser = new CSteamID(param.m_ulSteamIDUserChanged);
        var makingChange = new CSteamID(param.m_ulSteamIDMakingChange);

        var stateChange = (EChatMemberStateChange)param.m_rgfChatMemberStateChange;
        _logger.Debug("lobby member state changed: {LobbyId} {ChangedUser} {MakingChange} {StateChange}", lobbyId, changedUser, makingChange, stateChange);

        if (stateChange == EChatMemberStateChange.k_EChatMemberStateChangeEntered)
        {
            if (TryCreateSession(changedUser))
            {
                GameEventBus.Publish(new CreateSessionEvent(changedUser));
            }
        }
        else if (stateChange == EChatMemberStateChange.k_EChatMemberStateChangeLeft)
        {
            RemoveSession(changedUser);
        }
        else if (stateChange == EChatMemberStateChange.k_EChatMemberStateChangeDisconnected)
        {
            RemoveSession(changedUser);
        }
    }

    private void OnP2PSessionRequest(P2PSessionRequest_t param)
    {
        _logger.Warning("p2p session request: {SteamId}", param.m_steamIDRemote);

        if (_banned.Contains(param.m_steamIDRemote.m_SteamID.ToString(CultureInfo.InvariantCulture)))
        {
            _logger.Warning("banned player request: {SteamId}", param.m_steamIDRemote);
            SteamNetworking.CloseP2PSessionWithUser(param.m_steamIDRemote);
            return;
        }

        if (IsServerClosed())
        {
            _logger.Warning("server closed: {SteamId}", param.m_steamIDRemote);
            ServerClose(param.m_steamIDRemote);
            return;
        }

        SteamNetworking.AcceptP2PSessionWithUser(param.m_steamIDRemote);
    }

    #endregion
}