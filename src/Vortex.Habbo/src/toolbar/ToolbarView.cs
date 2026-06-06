// @see com.sulake.habbo.toolbar.ToolbarView (legacy toolbar system, pre-2024)

using System;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Runtime.Events;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Habbo.Toolbar.MeMenu;
using Vortex.Habbo.Window;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Toolbar;

/// @see com.sulake.habbo.toolbar.ToolbarView (legacy toolbar system — replaced by BottomBarLeft in 2024)
public class ToolbarView
{
    private const int ICON_REGION_WIDTH = 45;
    private const int WINDOW_RIGHT_PADDING = 10;
    private const int TOOLBAR_EXTENSION_MARGIN = 150;
    private const int COLLAPSED_MARGIN = 185;

    private IWindowContainer? _window;
    private readonly HabboToolbar _toolbar;
    private readonly IHabboWindowManager _windowManager;
    private MeMenuController? _meMenuController;
    private bool _disposed;

    /// @see ToolbarView.as::ToolbarView
    public ToolbarView(
        HabboToolbar param1,
        IHabboWindowManager param2,
        IAssetLibrary? param3,
        EventDispatcherWrapper? param4)
    {
        _ = param4;
        _toolbar = param1;
        _windowManager = param2;

        XmlAsset? xmlAsset = param3?.GetAssetByName("toolbar_view_xml") as XmlAsset;
        XElement? layoutXml = xmlAsset?.Content as XElement
            ?? HabboAssetResolver.LoadXmlAsset("toolbar_view_xml");

        if (layoutXml == null)
        {
            throw new InvalidOperationException("ToolbarView: toolbar_view_xml not found");
        }

        _window = param2.BuildFromXml(layoutXml, 1) as IWindowContainer;

        if (_window == null)
        {
            throw new InvalidOperationException("ToolbarView: failed to build toolbar_view window");
        }

        _window.AddEventListener(WindowEvent.WE_PARENT_RESIZED, OnParentResized);

        List<IWindow> toggleWindows = [];
        _window.GroupChildrenWithTag("TOGGLE", toggleWindows, -1);
        foreach (IWindow toggle in toggleWindows)
        {
            if (toggle is IRegionWindow region)
            {
                region.AddEventListener(WindowMouseEvent.CLICK, OnIconClick);
            }
        }

        // @see ToolbarView.as — create me menu controller (old system)
        _meMenuController = new MeMenuController(param1, this);

        CheckSize();
    }

    /// @see ToolbarView.as::get disposed
    public bool disposed => _disposed;

    /// @see ToolbarView.as::get window
    public IWindowContainer? window => _window;

    /// @see ToolbarView.as::get memenu
    public MeMenuController? memenu => _meMenuController;

    /// @see ToolbarView.as::dispose
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _meMenuController?.Dispose();
        _meMenuController = null;

        if (_window != null)
        {
            _window.Destroy();
            _window = null;
        }

        _disposed = true;
    }

    /// @see ToolbarView.as::setIconBitmap
    public void SetIconBitmap(int iconId, Image? bitmap)
    {
        if (bitmap == null || _window == null)
        {
            return;
        }

        if (iconId != HabboToolbarIconEnum.ICON_MEMENU)
        {
            return;
        }

        string? iconName = HabboToolbarIconEnum.GetIconName(iconId);

        if (iconName == null)
        {
            return;
        }

        if (_window.FindChildByName(iconName) is IBitmapWrapperWindow bitmapWin)
        {
            bitmapWin.Bitmap = bitmap;
        }
    }

    /// @see ToolbarView.as::iconVisibility
    public void IconVisibility(string iconName, bool visible)
    {
        if (_window?.FindChildByName(iconName) is IWindowContainer container)
        {
            container.visible = visible;
        }

        CheckSize();
    }

    /// @see ToolbarView.as::setToolbarState
    public void SetToolbarState(int state)
    {
        if (_window == null)
        {
            return;
        }

        _window.visible = state != HabboToolbarEnum.TOOLBAR_STATE_HIDDEN;

        if (!_window.visible)
        {
            return;
        }

        // @see ToolbarView.as — update icon visibility based on toolbar state
        bool inRoom = state == HabboToolbarEnum.TOOLBAR_STATE_ROOM_VIEW;
        SetIconVisibilityByState(inRoom);
        CheckSize();
    }

    private void SetIconVisibilityByState(bool inRoom)
    {
        if (_window == null)
        {
            return;
        }

        string? cameraName = HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_CAMERA);
        string? roomInfoName = HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_ROOMINFO);
        string? wiredMenuName = HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_WIRED_MENU);

        if (cameraName != null && _window.FindChildByName(cameraName) is IWindowContainer cam)
        {
            string camPos = _toolbar.GetProperty("camera.launch.ui.position");
            bool cameraAllowed = _toolbar.sessionDataManager?.IsPerkAllowed("CAMERA") ?? false;
            cam.visible = inRoom && camPos == "bottom-icons" && cameraAllowed;
        }

        if (roomInfoName != null && _window.FindChildByName(roomInfoName) is IWindowContainer roomInfo)
        {
            roomInfo.visible = inRoom;
        }

        if (wiredMenuName != null && _window.FindChildByName(wiredMenuName) is IWindowContainer wired)
        {
            // TODO(as3-port): IHabboUserDefinedRoomEvents.showToolbarMenuButton — not ported yet
            wired.visible = false;
        }
    }

    private void CheckSize()
    {
        if (_window == null)
        {
            return;
        }

        IWindow? desktop = _windowManager.GetWindowContext(1)?.GetDesktopWindow();

        if (desktop != null)
        {
            _window.y = (float)(desktop.height - _window.height);
        }

        _window.width = (ICON_REGION_WIDTH * CalculateNewWidth()) + WINDOW_RIGHT_PADDING + TOOLBAR_EXTENSION_MARGIN;
        _window.Invalidate();
    }

    private int CalculateNewWidth()
    {
        if (_window == null)
        {
            return 1;
        }

        List<IWindow> toggleWindows = [];
        _window.GroupChildrenWithTag("TOGGLE", toggleWindows, -1);

        int count = 1;
        foreach (IWindow w in toggleWindows)
        {
            if (w?.visible == true)
            {
                count++;
            }
        }

        return count;
    }

    private void OnParentResized(WindowEvent ev, IWindow window)
    {
        CheckSize();
    }

    private void OnIconClick(WindowEvent ev, IWindow window)
    {
        string name = window.name;
        if (!string.IsNullOrEmpty(name))
        {
            _toolbar.ToggleWindowVisibility(name);
            _windowManager.HideMatchingHint(name);
        }
    }
}
