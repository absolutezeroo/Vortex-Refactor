// @see habbo/window/widgets/RoomPreviewerWidget.as

using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Runtime.Events;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Iterators;
using Vortex.Habbo.Room;
using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Preview;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/RoomPreviewerWidget.as
public class RoomPreviewerWidget : IRoomPreviewerWidget, IWidget
{
    private static int _nextPreviewerId = 2;

    private IWidgetWindow? _widgetWindow;
    private HabboWindowManagerComponent? _windowManager;
    private IWindowContainer? _rootContainer;
    private IDisplayObjectWrapper? _roomCanvas;
    private RoomPreviewer? _roomPreviewer;
    private int _scale = RoomPreviewer.SCALE_NORMAL;
    private int _offsetX;
    private int _offsetY;
    private int _zoom = 1;

    /// @see habbo/window/widgets/RoomPreviewerWidget.as::RoomPreviewerWidget
    public RoomPreviewerWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;

        if (windowManager.RoomEngine() is not IRoomEngine roomEngine)
        {
            return;
        }

        if (roomEngine.Events is EventDispatcherWrapper roomEvents)
        {
            roomEvents.AddEventListener(RoomEngineEvent.ROOM_INITIALIZED, OnRoomInitialized);
        }

        // Build XML layout
        object? xmlAsset = windowManager.FindAssetByName("room_previewer_xml");

        if (xmlAsset is IAsset { Content: XElement xml })
        {
            _rootContainer = ((IWindowFactory)windowManager).BuildFromXml(xml) as IWindowContainer;
        }

        // Find room_canvas display object wrapper
        _roomCanvas = _rootContainer?.FindChildByName("room_canvas") as IDisplayObjectWrapper;

        // Create RoomPreviewer and initialize
        _roomPreviewer = new RoomPreviewer(roomEngine, _nextPreviewerId++);

        // Wire event listeners on root container
        if (_rootContainer is IWindow rootWindow)
        {
            rootWindow.AddEventListener(WindowMouseEvent.CLICK, OnClickRoomView);
            rootWindow.AddEventListener(WindowEvent.WE_RESIZE, OnResizeCanvas);
        }

        if (_rootContainer == null)
        {
            return;
        }

        // Attach layout to parent widget
        _widgetWindow.RootWindow(_rootContainer);
        _rootContainer.width = _widgetWindow.width;
        _rootContainer.height = _widgetWindow.height;

        // Initialize room canvas
        _roomPreviewer.ModifyRoomCanvas((int)_rootContainer.width, (int)_rootContainer.height);
    }

    /// @see habbo/window/widgets/RoomPreviewerWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/RoomPreviewerWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/RoomPreviewerWidget.as::scale
    int IRoomPreviewerWidget.Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            Refresh();
        }
    }

    /// @see habbo/window/widgets/RoomPreviewerWidget.as::roomPreviewer
    object? IRoomPreviewerWidget.RoomPreviewer => _roomPreviewer;

    /// @see habbo/window/widgets/RoomPreviewerWidget.as::offsetX
    int IRoomPreviewerWidget.OffsetX
    {
        get => _offsetX;
        set
        {
            _offsetX = value;
            Refresh();
        }
    }

    /// @see habbo/window/widgets/RoomPreviewerWidget.as::offsetY
    int IRoomPreviewerWidget.OffsetY
    {
        get => _offsetY;
        set
        {
            _offsetY = value;
            Refresh();
        }
    }

    /// @see habbo/window/widgets/RoomPreviewerWidget.as::zoom
    int IRoomPreviewerWidget.Zoom
    {
        get => _zoom;
        set
        {
            _zoom = value;
            Refresh();
        }
    }

    /// @see habbo/window/widgets/RoomPreviewerWidget.as::showPreview
    void IRoomPreviewerWidget.ShowPreview()
    {
        // TODO(window-port): AS3 sets a BitmapData image on the room canvas display wrapper.
        // In Godot, this would involve setting a texture on the preview display node.
    }

    /// @see habbo/window/widgets/RoomPreviewerWidget.as::onRoomInitialized
    private void OnRoomInitialized(object? param1)
    {
        if (param1 is not RoomEngineEvent roomEvent)
        {
            return;
        }

        if (roomEvent.Type != RoomEngineEvent.ROOM_INITIALIZED)
        {
            return;
        }

        if (_roomPreviewer == null || roomEvent.RoomId != _roomPreviewer.PreviewRoomId)
        {
            return;
        }

        _roomPreviewer.Reset(false);

        if (_roomCanvas == null)
        {
            return;
        }

        Node2D? canvas = _roomPreviewer.GetRoomCanvas((int)_roomCanvas.width, (int)_roomCanvas.height);

        if (canvas != null)
        {
            _roomCanvas.SetDisplayObject(canvas);
        }
    }

    /// @see habbo/window/widgets/RoomPreviewerWidget.as::refresh
    private void Refresh()
    {
        if (_roomPreviewer == null || !_roomPreviewer.IsRoomEngineReady)
        {
            return;
        }

        if (_scale == RoomPreviewer.SCALE_NORMAL)
        {
            _roomPreviewer.ZoomIn();
        }
        else
        {
            _roomPreviewer.ZoomOut();
        }

        _roomPreviewer.AddViewOffset = new Vector2(_offsetX, _offsetY);

        if (_roomCanvas?.GetDisplayObject() is Node2D displayObject)
        {
            displayObject.Scale = new Vector2(_zoom, _zoom);
            displayObject.Position = new Vector2(_offsetX, _offsetY);
        }
    }

    /// @see habbo/window/widgets/RoomPreviewerWidget.as::onResizeCanvas
    private void OnResizeCanvas(WindowEvent param1, IWindow param2)
    {
        int width = (int)param1.window.width;
        int height = (int)param1.window.height;
        
        _roomPreviewer?.ModifyRoomCanvas(width, height);
    }

    /// @see habbo/window/widgets/RoomPreviewerWidget.as::onClick
    private void OnClickRoomView(WindowEvent param1, IWindow param2)
    {
        _roomPreviewer?.ChangeRoomObjectState();
    }

    /// @see habbo/window/widgets/RoomPreviewerWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (_roomPreviewer != null)
        {
            _roomPreviewer.Dispose();
            _roomPreviewer = null;
        }

        if (_rootContainer != null)
        {
            (_rootContainer as IWindow)?.Destroy();
            _rootContainer = null;
        }

        if (_widgetWindow != null)
        {
            _widgetWindow.RootWindow(null);
            _widgetWindow = null;
        }

        if (_windowManager?.RoomEngine() is IRoomEngine roomEngine
            && roomEngine.Events is EventDispatcherWrapper roomEvents)
        {
            roomEvents.RemoveEventListener(RoomEngineEvent.ROOM_INITIALIZED, OnRoomInitialized);
        }

        _windowManager = null;
        _roomCanvas = null;
        disposed = true;
    }

    /// @see habbo/window/widgets/RoomPreviewerWidget.as::toString
    public override string ToString()
    {
        return "RoomPreviewerWidget";
    }
}
