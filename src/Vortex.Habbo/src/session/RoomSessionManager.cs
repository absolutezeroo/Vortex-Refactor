// @see com.sulake.habbo.session.RoomSessionManager

using System;

using Vortex.Core.Communication.Connection;
using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;
using Vortex.Habbo.Communication;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;
using Vortex.Habbo.Session.Events;
using Vortex.Habbo.Session.Handler;
using Vortex.IID;

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.RoomSessionManager
public class RoomSessionManager : Component, IRoomSessionManager, IRoomHandlerListener
{
    public const uint _SafeStr_1416 = 2;
    public const uint SETUP_WITHOUT_TRACKING = 3;
    public const uint _SafeStr_3712 = 4;

    private IHabboCommunicationManager? _communication;
    private readonly List<BaseHandler> _handlers = [];
    private bool _engineInitialized = false;
    private readonly Dictionary<string, RoomSession> _sessions = new(StringComparer.Ordinal);
    private RoomSession? _pendingSession;
    private bool _sessionStarting = false;
    // TODO(as3-port): IHabboTracking not yet ported
    // TODO(as3-port): IRoomEngine not yet ported
    // TODO(as3-port): IHabboFreeFlowChat not yet ported
    private bool _vizSettingsHandled = false;

    // @see RoomSessionManager.as::_SafeStr_581 — set from flags bit 0
    private readonly bool _useVisualizationSettings;

    // @see RoomSessionManager.as::_SafeStr_585 — content types awaiting load before start
    private List<string>? _pendingContentTypes;
    private RoomSession? _viewerSession;

    /// @see RoomSessionManager.as::RoomSessionManager
    public RoomSessionManager(IContext param1, uint param2 = 0, object? param3 = null)
        : base(param1, param2, param3)
    {
        _useVisualizationSettings = (param2 & 0x01) != 0;
        RegisterInterface(new IIDHabboRoomSessionManager(), this);
    }

    /// @see RoomSessionManager.as::get initialized
    public bool initialized => allRequiredDependenciesInjected && _engineInitialized;

    /// @see RoomSessionManager.as::get sessionStarting
    public bool sessionStarting => _sessionStarting;

    /// @see RoomSessionManager.as::get dependencies
    protected override IList<ComponentDependency> dependencies =>
        new List<ComponentDependency>(base.dependencies)
        {
            new(new IIDHabboCommunicationManager(), p => _communication = p as IHabboCommunicationManager,
                (flags & 0x02) == 0),
            new(new IIDHabboTracking(), null,
                (flags & 0x02) == 0),  // TODO(as3-port): IHabboTracking not yet ported
            new(new IIDHabboFreeFlowChat(), null, false),  // TODO(as3-port): IHabboFreeFlowChat not yet ported
            new(new IIDHabboConfigurationManager(), null, false),
            new(new IIDRoomEngine(), null, (flags & 0x04) == 0,
                new[] { new DependencyEventListener("REE_ENGINE_INITIALIZED", OnRoomEngineInitialized) }),
        };

    /// @see RoomSessionManager.as::initComponent
    protected override void InitComponent()
    {
        CreateHandlers();
        if (_useVisualizationSettings && _communication != null)
        {
            _communication.AddHabboConnectionMessageEvent(new RoomVisualizationSettingsEvent(OnRoomVisualizationSettings));
        }
        ExecutePendingSessionRequest();
    }

    /// @see RoomSessionManager.as::dispose
    public override void Dispose()
    {
        if (disposed)
        {
            return;
        }

        foreach (RoomSession session in _sessions.Values)
        {
            session.Dispose();
        }

        _sessions.Clear();
        foreach (BaseHandler handler in _handlers)
        {
            handler.Dispose();
        }

        _handlers.Clear();
        base.Dispose();
    }

    /// @see RoomSessionManager.as::onRoomEngineInitialized
    private void OnRoomEngineInitialized(object? ev)
    {
        _engineInitialized = true;

        ExecutePendingSessionRequest();
    }

    /// @see RoomSessionManager.as::onRoomVisualizationSettings
    private void OnRoomVisualizationSettings(object? ev)
    {
        if (_vizSettingsHandled || !_useVisualizationSettings)
        {
            return;
        }

        _vizSettingsHandled = true;
        // TODO(as3-port): IRoomEngine.createRoomCanvas / scene setup not yet ported
    }

    private void CreateHandlers()
    {
        IConnection? connection = _communication?.connection;
        _handlers.Add(new RoomSessionHandler(connection, this));
        _handlers.Add(new RoomChatHandler(connection, this));
        _handlers.Add(new RoomUsersHandler(connection, this));
        _handlers.Add(new RoomPermissionsHandler(connection, this));
        _handlers.Add(new AvatarEffectsHandler(connection, this));
        _handlers.Add(new RoomDataHandler(connection, this));
        _handlers.Add(new PresentHandler(connection, this));
        _handlers.Add(new GenericErrorHandler(connection, this));
        _handlers.Add(new PollHandler(connection, this));
        _handlers.Add(new WordQuizHandler(connection, this));
        _handlers.Add(new RoomDimmerPresetsHandler(connection, this));
        _handlers.Add(new PetPackageHandler(connection, this));
    }

    private void ExecutePendingSessionRequest()
    {
        if (initialized && _pendingSession != null)
        {
            CreateSession(_pendingSession);
            _pendingSession = null;
        }
    }

    /// @see RoomSessionManager.as::gotoRoom
    public bool GotoRoom(int roomId, string password = "", string doorbell = "")
    {
        RoomSession session = new RoomSession
        {
            roomId = roomId,
            roomPassword = password,
            roomResources = doorbell,
            // TODO(as3-port): habboTracking not yet ported
        };
        return CreateSession(session);
    }

    /// @see RoomSessionManager.as::gotoRoomNetwork
    public bool GotoRoomNetwork(int roomId, int networkId)
    {
        RoomSession session = new RoomSession
        {
            roomId = 1,
            roomPassword = "",
            // TODO(as3-port): RoomNetworkOpenConnectionMessageComposer not yet ported
            // session.openConnectionComposer = new RoomNetworkOpenConnectionMessageComposer(roomId, networkId);
        };
        return CreateSession(session);
    }

    private bool CreateSession(RoomSession session)
    {
        if (!initialized)
        {
            _pendingSession = session;
            return false;
        }
        string key = GetRoomIdentifier(session.roomId);
        _sessionStarting = true;
        if (_sessions.ContainsKey(key))
        {
            DisposeSession(session.roomId, false);
        }

        session.connection = _communication?.connection;
        _sessions[key] = session;
        events?.DispatchEvent(new RoomSessionEvent("RSE_CREATED", session));
        if (_useVisualizationSettings)
        {
            // TODO(as3-port): room engine content type tracking not yet ported
            _pendingContentTypes = [];
            _viewerSession = session;
            if (_pendingContentTypes.Count == 0)
            {
                StartSession(session);
            }
        }
        return true;
    }

    /// @see RoomSessionManager.as::startSession
    public bool StartSession(IRoomSession session)
    {
        if (session.state == "RSE_STARTED")
        {
            return false;
        }

        if (session.isGameSession)
        {
            return true;
        }

        if (session.Start())
        {
            _sessionStarting = false;
            events?.DispatchEvent(new RoomSessionEvent("RSE_STARTED", session));
            UpdateHandlers(session);
        }
        else
        {
            DisposeSession(session.roomId);
            _sessionStarting = false;
            return false;
        }
        return true;
    }

    /// @see RoomSessionManager.as::startGameSession
    public void StartGameSession()
    {
        RoomSession session = new RoomSession
        {
            roomId = 1,
            isGameSession = true,
        };
        session.connection = _communication?.connection;
        _sessions[GetRoomIdentifier(1)] = session;
        events?.DispatchEvent(new RoomSessionEvent("RSE_CREATED", session));
    }

    /// @see RoomSessionManager.as::disposeGameSession
    public void DisposeGameSession()
    {
        string key = GetRoomIdentifier(1);
        if (_sessions.TryGetValue(key, out RoomSession? session) && session.isGameSession)
        {
            DisposeSession(1, false);
        }
    }

    /// @see RoomSessionManager.as::sessionUpdate (IRoomHandlerListener)
    public void SessionUpdate(int roomId, string state)
    {
        IRoomSession? session = GetSession(roomId);
        if (session == null)
        {
            return;
        }

        switch (state)
        {
            case "RS_CONNECTED":
            case "RS_READY":
                return;
            case "RS_DISCONNECTED":
                DisposeSession(roomId);
                return;
        }
    }

    /// @see RoomSessionManager.as::sessionReinitialize (IRoomHandlerListener)
    public void SessionReinitialize(int oldRoomId, int newRoomId)
    {
        string oldKey = GetRoomIdentifier(oldRoomId);
        if (!_sessions.Remove(oldKey, out RoomSession? session))
        {
            return;
        }

        (session as RoomSession)?.Reset(newRoomId);
        string newKey = GetRoomIdentifier(newRoomId);
        _sessions.Remove(newKey);
        _sessions[newKey] = session;
        UpdateHandlers(session);
    }

    /// @see RoomSessionManager.as::getSession (IRoomHandlerListener + IRoomSessionManager)
    public IRoomSession? GetSession(int roomId)
    {
        _sessions.TryGetValue(GetRoomIdentifier(roomId), out RoomSession? session);
        return session;
    }

    /// @see RoomSessionManager.as::disposeSession
    public void DisposeSession(int roomId, bool sendDisconnect = true)
    {
        string key = GetRoomIdentifier(roomId);
        if (!_sessions.Remove(key, out RoomSession? session))
        {
            return;
        }

        events?.DispatchEvent(new RoomSessionEvent("RSE_ENDED", session, sendDisconnect));
        session.Dispose();
    }

    private void UpdateHandlers(IRoomSession session)
    {
        foreach (BaseHandler handler in _handlers)
        {
            handler.currentRoomId = session.roomId;
        }
    }

    /// @see RoomSessionManager.as::getRoomIdentifier — always returns same key per source
    private static string GetRoomIdentifier(int roomId)
    {
        return "hard_coded_room_id";
    }
}
