// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/habbo/ui/RoomUI.as

using System;
using System.Linq;

using Vortex.Core.Assets;
using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;
using Vortex.Core.Window.Components;
using Vortex.Habbo.Communication;
using Vortex.Habbo.Room;
using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Session;
using Vortex.Habbo.Session.Events;
using Vortex.Habbo.Window;
using Vortex.IID;

namespace Vortex.Habbo.UI;

/// @see com.sulake.habbo.ui.RoomUI
public class RoomUI : Component, IRoomUI, IUpdateReceiver
{
    private const string HARD_CODED_ROOM_ID = "hard_coded_room_id";

    private readonly Dictionary<string, RoomDesktop> _desktops = new(StringComparer.Ordinal);
    private IHabboCommunicationManager? _communication;
    private IHabboWindowManager? _windowManager;
    private IRoomEngine? _roomEngine;
    private IRoomSessionManager? _roomSessionManager;
    private bool _isInRoom;
    private int _lastRoomId = -1;
    private int _lastUserChooserState = -1;

    /// @see RoomUI.as::RoomUI
    public RoomUI(IContext param1, uint param2 = 0, object? param3 = null) : base(param1, param2, param3)
    {
        RegisterInterface(new IIDHabboRoomUI(), this);
    }

    /// @see RoomUI.as::get dependencies
    protected override IList<ComponentDependency> dependencies =>
        base.dependencies.Concat(
                [
                    new ComponentDependency(new IIDHabboWindowManager(), param1 => _windowManager = param1 as IHabboWindowManager),
                    new ComponentDependency(new IIDRoomEngine(), param1 => _roomEngine = param1 as IRoomEngine, true,
                        [
                            new DependencyEventListener(RoomEngineEvent.ROOM_ENGINE_INITIALIZED, RoomEngineEventHandler),
                            new DependencyEventListener(RoomEngineEvent.ROOM_INITIALIZED, RoomEventHandler),
                            new DependencyEventListener(RoomEngineEvent.ROOM_OBJECTS_INITIALIZED, RoomEngineEventHandler),
                            new DependencyEventListener(RoomEngineEvent.ROOM_DISPOSED, RoomEngineEventHandler),
                            new DependencyEventListener(RoomEngineEvent.ROOM_ENGINE_GAME_MODE, RoomEngineEventHandler),
                            new DependencyEventListener(RoomEngineEvent.ROOM_ENGINE_NORMAL_MODE, RoomEngineEventHandler),
                        ]),
                    new ComponentDependency(new IIDHabboRoomSessionManager(), param1 => _roomSessionManager = param1 as IRoomSessionManager,
                        true,
                        [
                            new DependencyEventListener(RoomSessionEvent.RSE_CREATED, RoomSessionStateEventHandler),
                            new DependencyEventListener(RoomSessionEvent.RSE_STARTED, RoomSessionStateEventHandler),
                            new DependencyEventListener(RoomSessionEvent.RSE_ENDED, RoomSessionStateEventHandler),
                            new DependencyEventListener(RoomSessionEvent.SESSION_ROOM_DATA, RoomSessionStateEventHandler),
                        ]),
                    new ComponentDependency(new IIDHabboCommunicationManager(), param1 => _communication = param1 as IHabboCommunicationManager,
                        false),
                ]
            )
            .ToList();

    /// @see RoomUI.as::initComponent
    protected override void InitComponent()
    {
        context.RegisterUpdateReceiver(this, 0);
        events.DispatchEvent("complete");
    }

    /// @see RoomUI.as::dispose
    public override void Dispose()
    {
        foreach (string identifier in _desktops.Keys.ToList())
        {
            DisposeDesktop(identifier);
        }

        context.RemoveUpdateReceiver(this);
        base.Dispose();
    }

    /// @see RoomUI.as::createDesktop
    public IRoomDesktop? CreateDesktop(IRoomSession? session)
    {
        if (session == null || _roomEngine == null)
        {
            return null;
        }

        string identifier = GetRoomIdentifier(session.roomId);

        if (_desktops.TryGetValue(identifier, out RoomDesktop? existing))
        {
            return existing;
        }

        RoomDesktop desktop = new(session, assets, _communication?.connection)
        {
            RoomEngine = _roomEngine,
            WindowManager = _windowManager,
            Layout = GetRoomDesktopLayout(),
        };

        desktop.CreateWidget("RWE_LOADINGBAR");
        desktop.CreateWidget("RWE_ROOM_QUEUE");
        desktop.Init();
        desktop.RequestInterstitial();

        _lastRoomId = session.roomId;
        _desktops[identifier] = desktop;

        return desktop;
    }

    /// @see IRoomUI.as::get chatContainer
    public IDisplayObjectWrapper? ChatContainer
    {
        get
        {
            if (_roomEngine == null)
            {
                return null;
            }

            return GetDesktop(GetRoomIdentifier(_roomEngine.ActiveRoomId)) is RoomDesktop desktop
                ? desktop.layoutManager.GetChatContainer()
                : null;
        }
    }

    /// @see RoomUI.as::disposeDesktop
    public void DisposeDesktop(string identifier)
    {
        if (!_desktops.Remove(identifier, out RoomDesktop? desktop))
        {
            return;
        }

        int userChooserState = desktop.GetWidgetState("RWE_USER_CHOOSER");

        if (userChooserState != -1)
        {
            _lastUserChooserState = userChooserState;
        }

        desktop.Dispose();
    }

    /// @see RoomUI.as::getDesktop
    public IRoomDesktop? GetDesktop(string identifier)
    {
        return _desktops.GetValueOrDefault(identifier);
    }

    /// @see RoomUI.as::getActiveCanvasId
    public int GetActiveCanvasId(int roomId)
    {
        _ = roomId;

        return 1;
    }

    /// @see RoomUI.as::set visible
    public bool Visible
    {
        set
        {
            if (_roomEngine == null)
            {
                return;
            }

            if (GetDesktop(GetRoomIdentifier(_roomEngine.ActiveRoomId)) is RoomDesktop desktop)
            {
                desktop.visible = value;
            }
        }
    }

    /// @see RoomUI.as::hideWidget
    public void HideWidget(string widgetType)
    {
        if (_roomEngine == null)
        {
            return;
        }

        GetDesktop(GetRoomIdentifier(_roomEngine.ActiveRoomId))?.ProcessEvent(widgetType);
    }

    /// @see RoomUI.as::showGamePlayerName
    public void ShowGamePlayerName(int userId, string userName, uint color, int roomId)
    {
        _ = userId;
        _ = userName;
        _ = color;
        _ = roomId;
        // TODO(as3-port): Game player name widget is not ported yet.
    }

    /// @see RoomUI.as::mouseEventPositionHasContextMenu
    public bool MouseEventPositionHasContextMenu(object? mouseEvent)
    {
        _ = mouseEvent;

        return false;
    }

    /// @see RoomUI.as::triggerbottomBarResize
    public void TriggerBottomBarResize()
    {
        // TODO(as3-port): Bottom bar widgets are not ported yet.
    }

    /// @see RoomUI.as::update
    public void Update(uint param1)
    {
        _ = param1;

        foreach (RoomDesktop desktop in _desktops.Values.ToList())
        {
            desktop.Update();
        }
    }

    /// @see RoomUI.as::roomSessionStateEventHandler
    private void RoomSessionStateEventHandler(object? eventObj)
    {
        if (_roomEngine == null || eventObj is not RoomSessionEvent sessionEvent)
        {
            return;
        }

        switch (sessionEvent.type)
        {
            case RoomSessionEvent.RSE_CREATED:
                CreateDesktop(sessionEvent.session);
                return;
            case RoomSessionEvent.RSE_STARTED:
            case RoomSessionEvent.SESSION_ROOM_DATA:
                return;
            case RoomSessionEvent.RSE_ENDED:
                DisposeDesktop(GetRoomIdentifier(sessionEvent.session.roomId));
                _isInRoom = false;
                return;
        }
    }

    /// @see RoomUI.as::roomEngineEventHandler
    private void RoomEngineEventHandler(object? eventObj)
    {
        if (eventObj is not RoomEngineEvent roomEvent)
        {
            return;
        }

        if (roomEvent.type is RoomEngineEvent.ROOM_ENGINE_GAME_MODE or RoomEngineEvent.ROOM_ENGINE_NORMAL_MODE)
        {
            GetDesktop(GetRoomIdentifier(roomEvent.roomId))?.ProcessEvent(roomEvent);
        }

        if (roomEvent.roomId == _lastRoomId && roomEvent.type == RoomEngineEvent.ROOM_DISPOSED)
        {
            _isInRoom = false;
        }
    }

    /// @see RoomUI.as::roomEventHandler
    private void RoomEventHandler(object? eventObj)
    {
        if (_roomEngine == null || eventObj is not RoomEngineEvent roomEvent)
        {
            return;
        }

        string identifier = GetRoomIdentifier(roomEvent.roomId);
        RoomDesktop? desktop = GetDesktop(identifier) as RoomDesktop;

        if (desktop == null)
        {
            IRoomSession? session = _roomSessionManager?.GetSession(roomEvent.roomId);

            if (session != null)
            {
                desktop = CreateDesktop(session) as RoomDesktop;
            }
        }

        if (desktop == null)
        {
            return;
        }

        if (roomEvent.type != RoomEngineEvent.ROOM_INITIALIZED)
        {
            desktop.ProcessEvent(roomEvent);
            return;
        }

        desktop.CreateRoomView(GetActiveCanvasId(roomEvent.roomId));
        _roomEngine.SetActiveRoom(roomEvent.roomId);
        desktop.DisposeWidget("RWE_ROOM_QUEUE");
        desktop.CreateWidget("RWE_CHAT_WIDGET");
        desktop.CreateWidget("RWE_INFOSTAND");
        desktop.CreateWidget("RWE_LOCATION_WIDGET");
        desktop.CreateWidget("RWE_ROOM_TOOLS");

        if (!desktop.roomSession.isSpectatorMode)
        {
            desktop.CreateWidget("RWE_ME_MENU");
            desktop.CreateWidget("RWE_CHAT_INPUT_WIDGET");
            desktop.CreateWidget("RWE_FRIEND_REQUEST");
        }

        desktop.CreateWidget("RWE_FURNI_PLACEHOLDER");
        desktop.CreateWidget("RWE_FURNI_CREDIT_WIDGET");
        desktop.CreateWidget("RWE_FURNI_STICKIE_WIDGET");
        desktop.CreateWidget("RWE_FURNI_PRESENT_WIDGET");
        desktop.CreateWidget("RWE_FURNI_TROPHY_WIDGET");
        desktop.CreateWidget("RWE_FURNI_ECOTRONBOX_WIDGET");
        desktop.CreateWidget("RWE_FURNI_PET_PACKAGE_WIDGET");
        desktop.CreateWidget("RWE_DOORBELL");
        desktop.CreateWidget("RWE_ROOM_POLL");
        desktop.CreateWidget("RWE_ROOM_DIMMER");
        desktop.CreateWidget("RWE_CLOTHING_CHANGE");
        desktop.CreateWidget("RWE_CONVERSION_TRACKING");
        desktop.CreateWidget("RWE_EFFECTS");
        desktop.CreateWidget("RWE_MANNEQUIN");
        desktop.CreateWidget("RWE_ROOM_BACKGROUND_COLOR");
        desktop.CreateWidget("RWE_CUSTOM_USER_NOTIFICATION");
        desktop.CreateWidget("RWE_FURNI_CHOOSER");
        desktop.CreateWidget("RWE_USER_CHOOSER");

        if (_lastUserChooserState != -1)
        {
            _lastUserChooserState = -1;
        }

        desktop.CreateWidget("RWE_PLAYLIST_EDITOR_WIDGET");
        desktop.CreateWidget("RWE_SPAMWALL_POSTIT_WIDGET");
        desktop.CreateWidget("RWE_FURNITURE_CONTEXT_MENU");
        desktop.CreateWidget("RWE_CAMERA");
        _isInRoom = true;
    }

    /// @see RoomUI.as::getRoomIdentifier
    private static string GetRoomIdentifier(int roomId)
    {
        _ = roomId;

        return HARD_CODED_ROOM_ID;
    }

    private System.Xml.Linq.XElement? GetRoomDesktopLayout()
    {
        if (assets is IAssetLibrary assetLibrary)
        {
            IAsset? asset = assetLibrary.GetAssetByName("room_desktop_layout_xml");

            if (asset?.Content is System.Xml.Linq.XElement xml)
            {
                return xml;
            }
        }

        return Window.Utils.HabboAssetResolver.LoadXmlAsset("room_desktop_layout_xml");
    }
}
