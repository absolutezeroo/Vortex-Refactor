// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/habbo/ui/RoomDesktop.as

using System;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Runtime.Events;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Habbo.Room;
using Vortex.Habbo.Session;
using Vortex.Habbo.Window;
using Vortex.Habbo.Window.Utils;
using Vortex.Room.Utils;

namespace Vortex.Habbo.UI;

/// @see com.sulake.habbo.ui.RoomDesktop
public sealed class RoomDesktop : IRoomDesktop, IDisposable
{
    private readonly object? _assets;
    private readonly List<int> _canvasIds = [];
    private IDisplayObjectWrapper? _roomCanvasWrapper;
    private IRoomEngine? _roomEngine;
    private IHabboWindowManager? _windowManager;
    private bool _disposed;

    /// @see RoomDesktop.as::RoomDesktop
    public RoomDesktop(IRoomSession session, object? assets, object? connection)
    {
        _ = connection;

        events = new EventDispatcherWrapper();
        roomSession = session;
        _assets = assets;
        layoutManager = new DesktopLayoutManager();
    }

    /// @see RoomDesktop.as::get events
    public EventDispatcherWrapper events { get; }

    /// @see RoomDesktop.as::get roomSession
    public IRoomSession roomSession { get; }

    /// @see RoomDesktop.as::get layoutManager
    public DesktopLayoutManager layoutManager { get; }

    /// @see RoomDesktop.as::set roomEngine
    public IRoomEngine? RoomEngine
    {
        set => _roomEngine = value;
    }

    /// @see RoomDesktop.as::set windowManager
    public IHabboWindowManager? WindowManager
    {
        set => _windowManager = value;
    }

    /// @see RoomDesktop.as::set layout
    public XElement? Layout
    {
        set => layoutManager.SetLayout(value, _windowManager, null);
    }

    /// @see RoomDesktop.as::set visible
    public bool visible
    {
        set
        {
            if (_roomCanvasWrapper != null)
            {
                _roomCanvasWrapper.visible = value;
            }
        }
    }

    /// @see RoomDesktop.as::init
    public void Init()
    {
        if (_roomEngine == null || roomSession == null)
        {
            return;
        }

        // AS3 initializes an empty content wait list here. Loading bar widgets are not ported yet.
    }

    /// @see RoomDesktop.as::requestInterstitial
    public void RequestInterstitial()
    {
        // TODO(as3-port): IAdManager is not ported yet.
    }

    /// @see RoomDesktop.as::createRoomView
    public void CreateRoomView(int canvasId)
    {
        Rect2? roomViewRect = layoutManager.RoomViewRect;

        if (roomViewRect == null)
        {
            return;
        }

        int width = (int)roomViewRect.Value.Size.X;
        int height = (int)roomViewRect.Value.Size.Y;
        int scale = roomSession.isGameSession ? 32 : 64;

        if (_canvasIds.Contains(canvasId))
        {
            return;
        }

        if (_windowManager == null || _roomEngine == null)
        {
            return;
        }

        Node2D? canvas = _roomEngine.CreateRoomCanvas(roomSession.roomId, canvasId, width, height, scale);

        if (canvas == null)
        {
            return;
        }

        ApplyInitialGeometry(canvasId);

        XElement? roomViewXml = GetXmlAsset("room_view_container_xml");

        if (roomViewXml == null)
        {
            return;
        }

        IWindowContainer? roomViewContainer = ((IWindowFactory)_windowManager).BuildFromXml(roomViewXml) as IWindowContainer;

        if (roomViewContainer == null)
        {
            return;
        }

        roomViewContainer.width = width;
        roomViewContainer.height = height;

        _roomCanvasWrapper = roomViewContainer.FindChildByName("room_canvas_wrapper") as IDisplayObjectWrapper;

        if (_roomCanvasWrapper == null)
        {
            return;
        }

        _roomCanvasWrapper.SetDisplayObject(canvas);
        layoutManager.AddRoomView(roomViewContainer);
        _canvasIds.Add(canvasId);
    }

    /// @see RoomDesktop.as::processEvent
    public void ProcessEvent(object? eventObj)
    {
        events.DispatchEvent(eventObj);
    }

    /// @see RoomDesktop.as::createWidget
    public void CreateWidget(string widgetType)
    {
        _ = widgetType;
        // TODO(as3-port): Room widgets are not ported yet.
    }

    /// @see RoomDesktop.as::disposeWidget
    public void DisposeWidget(string widgetType)
    {
        _ = widgetType;
        // TODO(as3-port): Room widgets are not ported yet.
    }

    /// @see RoomDesktop.as::getWidgetState
    public int GetWidgetState(string widgetType)
    {
        _ = widgetType;

        return -1;
    }

    /// @see RoomDesktop.as::update
    public void Update()
    {
        // TODO(as3-port): Widget update listeners and mouse-zoom inertia are not ported yet.
    }

    /// @see RoomDesktop.as::dispose
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        layoutManager.Dispose();
        events.Dispose();
        _canvasIds.Clear();
        _roomCanvasWrapper = null;
        _roomEngine = null;
        _windowManager = null;
        _disposed = true;
    }

    /// @see RoomDesktop.as::createRoomView — initial RoomGeometry centering block
    private void ApplyInitialGeometry(int canvasId)
    {
        if (_roomEngine?.GetRoomCanvasGeometry(roomSession.roomId, canvasId) is not RoomGeometry geometry)
        {
            return;
        }

        double minX = _roomEngine.GetRoomNumberValue(roomSession.roomId, "room_min_x");
        double maxX = _roomEngine.GetRoomNumberValue(roomSession.roomId, "room_max_x");
        double minY = _roomEngine.GetRoomNumberValue(roomSession.roomId, "room_min_y");
        double maxY = _roomEngine.GetRoomNumberValue(roomSession.roomId, "room_max_y");
        double local13 = (minX + maxX) / 2;
        double local10 = (minY + maxY) / 2;
        double local8 = 20;

        local13 += local8 - 1;
        local10 += local8 - 1;

        double local11 = Math.Sqrt((local8 * local8) + (local8 * local8)) * Math.Tan(Math.PI / 6);

        geometry.Location = new Vector3d(local13, local10, local11);
    }

    private XElement? GetXmlAsset(string assetName)
    {
        if (_assets is IAssetLibrary assetLibrary)
        {
            IAsset? asset = assetLibrary.GetAssetByName(assetName);

            if (asset?.Content is XElement xml)
            {
                return xml;
            }
        }

        return HabboAssetResolver.LoadXmlAsset(assetName);
    }
}
