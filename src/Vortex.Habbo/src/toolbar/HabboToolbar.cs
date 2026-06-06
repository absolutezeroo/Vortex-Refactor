// @see com.sulake.habbo.toolbar.HabboToolbar

using System;

using Godot;

using Vortex.Core.Communication.Messages;
using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Habbo.Communication;
using Vortex.Habbo.Toolbar.Events;
using Vortex.Habbo.Window;
using Vortex.IID;

namespace Vortex.Habbo.Toolbar;

/// @see com.sulake.habbo.toolbar.HabboToolbar
public class HabboToolbar : Component, IHabboToolbar
{
    private IHabboCommunicationManager? _communication;
    private IHabboWindowManager? _windowManager;

    // TODO(as3-port): IHabboLocalizationManager — not ported yet
    // TODO(as3-port): IHabboCatalog — not ported yet
    // TODO(as3-port): IHabboInventory — not ported yet
    // TODO(as3-port): IHabboNavigator / IHabboNewNavigator — not ported yet
    // TODO(as3-port): IAvatarRenderManager — optional wiring
    // TODO(as3-port): ISessionDataManager — optional wiring
    // TODO(as3-port): IHabboHelp — not ported yet
    // TODO(as3-port): IHabboMessenger — not ported yet
    // TODO(as3-port): IHabboQuestEngine — not ported yet
    // TODO(as3-port): IHabboRoomUI — optional wiring
    // TODO(as3-port): IHabboFreeFlowChat — not ported yet
    // TODO(as3-port): IHabboUserDefinedRoomEvents — not ported yet

    private const uint TOOLBAR_LAYER = 1;
    private const int ICON_REGION_WIDTH = 45;
    private const int WINDOW_RIGHT_PADDING = 10;
    private const int TOOLBAR_EXTENSION_MARGIN = 150;
    private const int COLLAPSED_MARGIN = 185;

    private int _currentState = HabboToolbarEnum.TOOLBAR_STATE_HOTEL_VIEW;
    private readonly Dictionary<int, bool> _iconVisibility = new();
    private bool _onDuty;

    private BottomBarLeft? _bottomBarLeft;
    private IWindowContainer? toolbarWindow => _bottomBarLeft?.window;
    private IDesktopWindow? _desktopWindow;

    /// @see com.sulake.habbo.toolbar.HabboToolbar::HabboToolbar
    public HabboToolbar(IContext param1, uint param2 = 0, object? param3 = null)
        : base(param1, param2, param3)
    {
        RegisterInterface(new IIDHabboToolbar(), this);
    }

    /// @see com.sulake.habbo.toolbar.HabboToolbar::get dependencies
    protected override IList<ComponentDependency> dependencies =>
        new List<ComponentDependency>(base.dependencies)
        {
            new(new IIDHabboCommunicationManager(), p => _communication = p as IHabboCommunicationManager),
            new(new IIDHabboWindowManager(), p =>
            {
                _windowManager = p as IHabboWindowManager;
                // InitComponent may have already run before the window manager was resolved (optional dep);
                // create the toolbar view now if that's the case.
                if (!locked && _bottomBarLeft == null)
                {
                    CreateToolbarView();
                }
            }, false),
            // TODO(as3-port): Add optional deps when their IIDs are ported
        };

    /// @see com.sulake.habbo.toolbar.HabboToolbar::initComponent
    protected override void InitComponent()
    {
        InitDefaultIconVisibility();
        CreateToolbarView();
    }

    /// @see com.sulake.habbo.toolbar.HabboToolbar::dispose
    public override void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (_bottomBarLeft != null)
        {
            _bottomBarLeft.Dispose();
            _bottomBarLeft = null;
        }

        if (_desktopWindow != null)
        {
            _desktopWindow.RemoveEventListener(WindowEvent.WE_RESIZED, OnDesktopResized);
            _desktopWindow = null;
        }

        _iconVisibility.Clear();
        base.Dispose();
    }

    // --- IHabboToolbar ---

    /// @see IHabboToolbar.as::get toolBarAreaWidth
    public int toolBarAreaWidth
    {
        get
        {
            if (toolbarWindow == null)
            {
                return 0;
            }

            if (_currentState == HabboToolbarEnum.TOOLBAR_STATE_COLLAPSED)
            {
                return COLLAPSED_MARGIN;
            }

            IWindow? line = toolbarWindow.FindChildByName("line");

            return line?.parent != null
                ? (int)(line.x + line.parent.x)
                : 0;
        }
    }

    /// @see IHabboToolbar.as::get onDuty
    public bool onDuty => _onDuty;

    /// @see com.sulake.habbo.toolbar.HabboToolbar::setToolbarState
    public void SetToolbarState(int state)
    {
        if (_currentState == state)
        {
            return;
        }

        _currentState = state;
        UpdateIconVisibilityForState(state);

        if (toolbarWindow != null)
        {
            toolbarWindow.visible = state != HabboToolbarEnum.TOOLBAR_STATE_HIDDEN;
        }

        events?.DispatchEvent(new HabboToolbarEvent(HabboToolbarEvent.RESIZED));
    }

    /// @see com.sulake.habbo.toolbar.HabboToolbar::toggleWindowVisibility
    public bool ToggleWindowVisibility(string windowId)
    {
        int iconId = HabboToolbarIconEnum.GetIconId(windowId);
        events?.DispatchEvent(new HabboToolbarEvent(HabboToolbarEvent.TOOLBAR_CLICK, iconId, windowId));
        return true;
    }

    /// @see com.sulake.habbo.toolbar.HabboToolbar::setIconBitmap
    public void SetIconBitmap(int iconId, Image? bitmap)
    {
        // @see HabboToolbar.as::setIconBitmap — forward to BottomBarLeft; AS3 passes original (not clone)
        _bottomBarLeft?.SetIconBitmap(iconId, bitmap);
    }

    /// @see com.sulake.habbo.toolbar.HabboToolbar::getRect
    public Rect2I GetRect()
    {
        if (toolbarWindow == null)
        {
            return default;
        }

        return new Rect2I(
            (int)toolbarWindow.renderingX,
            (int)toolbarWindow.renderingY,
            (int)toolbarWindow.renderingWidth,
            (int)toolbarWindow.renderingHeight
        );
    }

    /// @see com.sulake.habbo.toolbar.HabboToolbar::setIconVisibility
    public void SetIconVisibility(int iconId, bool visible)
    {
        _iconVisibility[iconId] = visible;
        ApplyIconVisibility(iconId, visible);
        CheckSize();
    }

    /// @see com.sulake.habbo.toolbar.HabboToolbar::createTransitionToIcon
    public void CreateTransitionToIcon(int iconId)
    {
        // TODO(as3-port): animate bounce transition on icon region
    }

    /// @see com.sulake.habbo.toolbar.HabboToolbar::getIconLocation
    public Vector2I GetIconLocation(int iconId)
    {
        if (toolbarWindow == null)
        {
            return Vector2I.Zero;
        }

        string? iconName = HabboToolbarIconEnum.GetIconName(iconId);

        if (iconName == null)
        {
            return Vector2I.Zero;
        }

        IWindow? region = toolbarWindow.FindChildByName(iconName.ToUpperInvariant());

        return region != null
            ? new Vector2I((int)region.renderingX, (int)region.renderingY)
            : Vector2I.Zero;
    }

    // --- Private helpers ---

    /// @see com.sulake.habbo.toolbar.HabboToolbar::initComponent — delegates to BottomBarLeft
    private void CreateToolbarView()
    {
        if (_windowManager == null || _bottomBarLeft != null)
        {
            return;
        }

        // @see HabboToolbar.as::initComponent — new BottomBarLeft(this, _windowManager, assets, events)
        _bottomBarLeft = new BottomBarLeft(
            this,
            _windowManager,
            assets as Core.Assets.IAssetLibrary,
            events);

        if (toolbarWindow != null)
        {
            toolbarWindow.procedure = OnToolbarWindowProcedure;
        }

        AttachToDesktop();
        SyncAllIconVisibility();
    }

    private void AttachToDesktop()
    {
        if (toolbarWindow == null || _windowManager == null)
        {
            return;
        }

        IDesktopWindow? desktop = _windowManager.GetWindowContext(TOOLBAR_LAYER)?.GetDesktopWindow();

        if (desktop != null)
        {
            if (_desktopWindow != desktop)
            {
                _desktopWindow?.RemoveEventListener(WindowEvent.WE_RESIZED, OnDesktopResized);
                _desktopWindow = desktop;
                _desktopWindow.AddEventListener(WindowEvent.WE_RESIZED, OnDesktopResized);
            }

            if (toolbarWindow.parent == null)
            {
                desktop.AddChild(toolbarWindow);
            }

            PositionToolbar(desktop);
        }
    }

    /// Push current _iconVisibility state to the window children by name.
    private void SyncAllIconVisibility()
    {
        foreach (KeyValuePair<int, bool> entry in _iconVisibility)
        {
            ApplyIconVisibility(entry.Key, entry.Value);
        }

        CheckSize();
    }

    /// Push one icon's visibility to the matching window region.
    private void ApplyIconVisibility(int iconId, bool visible)
    {
        if (toolbarWindow == null)
        {
            return;
        }

        string? iconName = HabboToolbarIconEnum.GetIconName(iconId);

        if (iconName == null)
        {
            return;
        }

        IWindow? region = toolbarWindow.FindChildByName(iconName.ToUpperInvariant());

        if (region != null)
        {
            region.visible = visible;
        }
    }

    /// @see com.sulake.habbo.toolbar.BottomBarLeft::checkSize
    private void CheckSize()
    {
        if (toolbarWindow == null)
        {
            return;
        }

        if (toolbarWindow.FindChildByName("toolbar_items") is BoxSizerController toolbarItems)
        {
            toolbarItems.ArrangeChildren();
        }

        toolbarWindow.width = ICON_REGION_WIDTH * CalculateNewWidth() + WINDOW_RIGHT_PADDING + TOOLBAR_EXTENSION_MARGIN;
        PositionToolbar(_desktopWindow);
        toolbarWindow.Invalidate();
    }

    /// @see com.sulake.habbo.toolbar.BottomBarLeft::calculateNewWidth
    private int CalculateNewWidth()
    {
        if (toolbarWindow == null)
        {
            return 1;
        }

        List<IWindow> windows = new();
        toolbarWindow.GroupChildrenWithTag("TOGGLE", windows, -1);

        int count = 1;

        foreach (IWindow window in windows)
        {
            if (window.visible)
            {
                count++;
            }
        }

        return count;
    }

    /// @see com.sulake.habbo.toolbar.BottomBarLeft::checkSize
    private void PositionToolbar(IDesktopWindow? desktop)
    {
        if (toolbarWindow == null || desktop == null)
        {
            return;
        }

        Vector2 desktopSize = desktop.rectangle.Size;

        toolbarWindow.x = 0f;
        toolbarWindow.y = Math.Max(0f, desktopSize.Y - toolbarWindow.height);
    }

    private void OnDesktopResized(WindowEvent ev, IWindow window)
    {
        _ = ev;
        _ = window;

        CheckSize();
    }

    /// @see com.sulake.habbo.toolbar.HabboToolbar — window procedure for icon clicks
    private void OnToolbarWindowProcedure(WindowEvent ev, IWindow window)
    {
        if (!string.Equals(ev.type, WindowEvent.WE_ACTIVATE, StringComparison.Ordinal))
        {
            return;
        }

        string? regionName = ev.window?.name;

        if (string.IsNullOrEmpty(regionName))
        {
            return;
        }

        ToggleWindowVisibility(regionName.ToLowerInvariant());
    }

    /// @see com.sulake.habbo.toolbar.HabboToolbar — default icon visibility per AS3 initComponent
    private void InitDefaultIconVisibility()
    {
        _iconVisibility[HabboToolbarIconEnum.ICON_NAVIGATOR]    = true;
        _iconVisibility[HabboToolbarIconEnum.ICON_CATALOGUE]    = true;
        _iconVisibility[HabboToolbarIconEnum.ICON_INVENTORY]    = true;
        _iconVisibility[HabboToolbarIconEnum.ICON_MEMENU]       = true;
        _iconVisibility[HabboToolbarIconEnum.ICON_HELP]         = true;
        _iconVisibility[HabboToolbarIconEnum.ICON_QUESTS]       = false;
        _iconVisibility[HabboToolbarIconEnum.ICON_ACHIEVEMENTS] = false;
        _iconVisibility[HabboToolbarIconEnum.ICON_GAMES]        = false;
        _iconVisibility[HabboToolbarIconEnum.ICON_STORIES]      = false;
        _iconVisibility[HabboToolbarIconEnum.ICON_GUIDE]        = false;
        _iconVisibility[HabboToolbarIconEnum.ICON_BUILDER]      = false;
        _iconVisibility[HabboToolbarIconEnum.ICON_CAMERA]       = false;
        _iconVisibility[HabboToolbarIconEnum.ICON_WIRED_MENU]   = false;
        _iconVisibility[HabboToolbarIconEnum.ICON_ROOMINFO]     = false;
        _iconVisibility[HabboToolbarIconEnum.ICON_GROUP]        = false;
        _iconVisibility[HabboToolbarIconEnum.ICON_HOME]         = false;
        _iconVisibility[HabboToolbarIconEnum.ICON_RECEPTION]    = false;
    }

    /// @see com.sulake.habbo.toolbar.HabboToolbar — update icon visibility on state change
    private void UpdateIconVisibilityForState(int state)
    {
        bool inRoom = state == HabboToolbarEnum.TOOLBAR_STATE_ROOM_VIEW;

        SetIconVisibility(HabboToolbarIconEnum.ICON_ROOMINFO,   inRoom);
        SetIconVisibility(HabboToolbarIconEnum.ICON_WIRED_MENU, inRoom);
        SetIconVisibility(HabboToolbarIconEnum.ICON_CAMERA,     inRoom);
        SetIconVisibility(HabboToolbarIconEnum.ICON_BUILDER,    inRoom);
        SetIconVisibility(HabboToolbarIconEnum.ICON_GROUP,      inRoom);
    }
}
