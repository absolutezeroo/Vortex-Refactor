// @see WIN63-202407091256-704579380-Source-main/habbo/toolbar/BottomBarLeft.as

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

/// @see WIN63-202407091256-704579380-Source-main/habbo/toolbar/BottomBarLeft.as
public class BottomBarLeft : ILinkEventTracker
{
    // @see BottomBarLeft.as::ICON_MOUSE_OVER, ICON_MOUSE_OUT
    private const string ICON_MOUSE_OVER = "_hover";
    private const string ICON_MOUSE_OUT = "_normal";

    // @see BottomBarLeft.as::ME_MENU_ICON_NAME
    private const string ME_MENU_ICON_NAME = "icon_me_menu";

    // @see BottomBarLeft.as::ICON_REGION_WIDTH, WINDOW_RIGHT_PADDING, COLLAPSED_MARGIN
    private const int ICON_REGION_WIDTH = 45;
    private const int WINDOW_RIGHT_PADDING = 10;
    private const int COLLAPSED_MARGIN = 185;
    private const int TOOLBAR_EXTENSION_MARGIN = 150;

    // @see BottomBarLeft.as::ICON_BG_COLOR_OVER, ICON_BG_COLOR_OUT
    private const uint ICON_BG_COLOR_OVER = 7433577;
    private const uint ICON_BG_COLOR_OUT = 5723213;

    /// @see BottomBarLeft.as::_window
    private IWindowContainer? _window;

    private readonly HabboToolbar _toolbar;
    private readonly IHabboWindowManager _windowManager;
    private readonly IAssetLibrary? _assets;

    /// @see BottomBarLeft.as::var_1899 (new items label)
    private IWindowContainer? _newItemsLabel;

    /// @see BottomBarLeft.as::_buttonContainer
    private IWindowContainer? _buttonContainer;

    /// @see BottomBarLeft.as::_left_arrow
    private IRegionWindow? _leftArrow;

    /// @see BottomBarLeft.as::_right_arrow
    private IRegionWindow? _rightArrow;

    /// @see BottomBarLeft.as::var_3539 (line separator)
    private IWindow? _separator;

    /// @see BottomBarLeft.as::var_3480 (saved pre-collapse state)
    private int _savedState = HabboToolbarEnum.TOOLBAR_STATE_HOTEL_VIEW;

    /// @see BottomBarLeft.as::var_2202 (collapsed flag)
    private bool _isCollapsed;

    /// @see BottomBarLeft.as::var_4515
    private bool _newItemsNotificationEnabled;

    /// @see BottomBarLeft.as::var_2595 (me-menu avatar bitmap)
    private Image? _meMenuBitmap;

    /// @see BottomBarLeft.as::_disposed
    private bool _disposed;

    /// @see BottomBarLeft.as::var_2280
    private MeMenuNewController? _meMenuController;

    // @see BottomBarLeft.as::var_4482, var_4400, var_4359
    private int _unseenMiniMailCount;
    private int _unseenAchievementCount;
    private int _unseenForumsCount;

    /// @see BottomBarLeft.as::_unseenItemCounters
    private readonly Dictionary<string, IWindowContainer> _unseenItemCounters = new();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/toolbar/BottomBarLeft.as::BottomBarLeft
    public BottomBarLeft(
        HabboToolbar param1,
        IHabboWindowManager param2,
        IAssetLibrary? param3,
        EventDispatcherWrapper? param4)
    {
        _ = param4;
        _toolbar = param1;
        _windowManager = param2;
        _assets = param3;

        // @see BottomBarLeft.as::BottomBarLeft — create me menu controller
        _meMenuController = new MeMenuNewController(param1, this);

        // @see BottomBarLeft.as::BottomBarLeft — get layout XML from component assets
        XmlAsset? xmlAsset = param3?.GetAssetByName("bottom_bar_left_xml") as XmlAsset;
        XElement? layoutXml = xmlAsset?.Content as XElement
            ?? HabboAssetResolver.LoadXmlAsset("bottom_bar_left_xml"); // Godot adaptation: fallback when asset library not loaded

        if (layoutXml == null)
        {
            throw new InvalidOperationException("BottomBarLeft: bottom_bar_left_xml not found");
        }

        _window = param2.BuildFromXml(layoutXml, 1) as IWindowContainer;

        if (_window == null)
        {
            throw new InvalidOperationException("Failed to construct window from XML!");
        }

        _window.AddEventListener(WindowEvent.WE_PARENT_RESIZED, OnParentResized);

        _buttonContainer = _window.GetChildByName("toolbar_items") as IWindowContainer;

        IWindowContainer? arrowLeft = _window.GetChildByName("arrow_container_left") as IWindowContainer;
        IWindowContainer? arrowRight = _window.GetChildByName("arrow_container_right") as IWindowContainer;
        _leftArrow = arrowLeft?.GetChildByName("collapse_left") as IRegionWindow;
        _rightArrow = arrowRight?.GetChildByName("collapse_right") as IRegionWindow;

        if (_leftArrow != null)
        {
            _leftArrow.AddEventListener(WindowMouseEvent.CLICK, OnCollapseToolsBar);
        }

        if (_rightArrow != null)
        {
            _rightArrow.AddEventListener(WindowMouseEvent.CLICK, OnCollapseToolsBar);
        }

        _separator = _buttonContainer?.GetChildByName("line") as IWindow;

        // @see BottomBarLeft.as — register WME_CLICK on all TOGGLE children
        List<IWindow> toggleWindows = [];
        _window.GroupChildrenWithTag("TOGGLE", toggleWindows, -1);
        foreach (IWindow toggle in toggleWindows)
        {
            if (toggle is IRegionWindow region)
            {
                region.AddEventListener(WindowMouseEvent.CLICK, OnIconClick);
            }
        }

        // @see BottomBarLeft.as::BottomBarLeft — initial icon visibility
        IconVisibility(HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_MEMENU)!, false);
        IconVisibility(HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_INVENTORY)!, false);
        IconVisibility(HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_WIRED_MENU)!, false);
        bool gamesEnabled = param1.GetBoolean("games_icon_enabled");
        IconVisibility(HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_GAMES)!, gamesEnabled);

        // @see BottomBarLeft.as::BottomBarLeft — build new items label
        XmlAsset? labelXmlAsset = param3?.GetAssetByName("new_items_label_xml") as XmlAsset;
        XElement? labelXml = labelXmlAsset?.Content as XElement
            ?? HabboAssetResolver.LoadXmlAsset("new_items_label_xml"); // Godot adaptation

        if (labelXml == null)
        {
            throw new InvalidOperationException("BottomBarLeft: new_items_label_xml not found");
        }

        _newItemsLabel = param2.BuildFromXml(labelXml, 2) as IWindowContainer;

        if (_newItemsLabel == null)
        {
            throw new InvalidOperationException("Failed to construct toolbar label from XML!");
        }

        // @see BottomBarLeft.as — register hint windows for NAVIGATOR, MEMENU, INVENTORY
        RegisterHintIfPresent(HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_NAVIGATOR)!);
        RegisterHintIfPresent(HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_MEMENU)!);
        RegisterHintIfPresent(HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_INVENTORY)!);

        // @see BottomBarLeft.as — add label to CATALOGUE container and register its hint
        string catalogueName = HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_CATALOGUE)!;
        IWindowContainer? catalogueContainer = _window.FindChildByName(catalogueName) as IWindowContainer;

        if (catalogueContainer != null)
        {
            catalogueContainer.AddChild(_newItemsLabel);
            _windowManager.RegisterHintWindow(catalogueName, catalogueContainer);

            // TODO(as3-port): ILocalization — localization not ported; skip label text
            _newItemsLabel.visible = false;
            _newItemsLabel.x = catalogueContainer.width - _newItemsLabel.width;
            _newItemsLabel.y = 0;
        }

        _newItemsNotificationEnabled = IsNewItemsNotificationEnabled();
        CheckSize();

        // @see BottomBarLeft.as — register as link event tracker
        param1.context?.AddLinkEventTracker(this);
    }

    /// @see BottomBarLeft.as::get disposed
    public bool disposed => _disposed;

    /// @see BottomBarLeft.as::get window
    public IWindowContainer? window => _window;

    /// @see BottomBarLeft.as::dispose
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        // @see BottomBarLeft.as::dispose
        if (_meMenuController != null)
        {
            _meMenuController.Dispose();
            _meMenuController = null;
        }

        _meMenuBitmap = null;
        _unseenItemCounters.Clear();

        if (_window != null)
        {
            _window.Destroy();
            _window = null;
        }

        if (_newItemsLabel != null)
        {
            _newItemsLabel.Destroy();
            _newItemsLabel = null;
        }

        _windowManager.UnregisterHintWindow(HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_NAVIGATOR)!);
        _windowManager.UnregisterHintWindow(HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_MEMENU)!);
        _windowManager.UnregisterHintWindow(HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_INVENTORY)!);
        _windowManager.UnregisterHintWindow(HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_CATALOGUE)!);

        _toolbar.context?.RemoveLinkEventTracker(this);
        _disposed = true;
    }

    /// @see BottomBarLeft.as::setIconBitmap
    public void SetIconBitmap(int iconId, Image? bitmap)
    {
        if (bitmap == null)
        {
            return;
        }

        // @see BottomBarLeft.as — only HTIE_ICON_MEMENU has explicit bitmap handling; all other icons are skin-driven
        if (iconId != HabboToolbarIconEnum.ICON_MEMENU)
        {
            return;
        }

        SetMeMenuIconBitmaps(bitmap);

        IWindow? iconBmp = _window?.FindChildByName(ME_MENU_ICON_NAME);
        if (iconBmp != null)
        {
            SetIconHoverState(iconBmp, ICON_MOUSE_OUT);
        }
    }

    /// @see BottomBarLeft.as::iconVisibility
    public void IconVisibility(string iconName, bool visible)
    {
        if (_window?.FindChildByName(iconName) is IWindowContainer container)
        {
            container.visible = visible;
        }

        CheckSize();
    }

    /// @see BottomBarLeft.as::setToolbarState
    public void SetToolbarState(int state)
    {
        if (_window == null)
        {
            return;
        }

        if (state == HabboToolbarEnum.TOOLBAR_STATE_HIDDEN)
        {
            _window.visible = false;
            return;
        }

        _window.visible = true;

        if (state != HabboToolbarEnum.TOOLBAR_STATE_COLLAPSED)
        {
            _savedState = state;
        }

        string? visTag = GetStateVisibilityTag(state);

        bool inRoom = state is HabboToolbarEnum.TOOLBAR_STATE_ROOM_VIEW
                or HabboToolbarEnum.TOOLBAR_STATE_NOOB_HOME
                or HabboToolbarEnum.TOOLBAR_STATE_NOOB_NOT_HOME
            || (_isCollapsed && _savedState is HabboToolbarEnum.TOOLBAR_STATE_ROOM_VIEW
                or HabboToolbarEnum.TOOLBAR_STATE_NOOB_HOME
                or HabboToolbarEnum.TOOLBAR_STATE_NOOB_NOT_HOME);

        List<IWindow> toggleWindows = [];
        _window.GroupChildrenWithTag("TOGGLE", toggleWindows, -1);

        foreach (IWindow toggle in toggleWindows)
        {
            if (toggle == null)
            {
                continue;
            }

            toggle.visible = visTag != null && toggle.tags.Contains(visTag);

            string name = toggle.name ?? string.Empty;

            if (!_isCollapsed && name == "QUESTS")
            {
                toggle.visible &= !_toolbar.GetBoolean("toolbar.hide.quests");
            }
            else if (!_isCollapsed && name == "STORIES")
            {
                toggle.visible &= _toolbar.GetBoolean("toolbar.stories.enabled");
            }
            else if (!_isCollapsed && name == "BUILDER")
            {
                toggle.visible &= _toolbar.GetBoolean("builders.club.enabled");
            }
            else if (name == "GAMES")
            {
                toggle.visible &= _toolbar.GetBoolean("games_icon_enabled");
            }
            else if (name == "CAMERA")
            {
                string camPos = _toolbar.GetProperty("camera.launch.ui.position");
                // TODO(as3-port): ISessionDataManager.isPerkAllowed("CAMERA") — not ported yet
                toggle.visible = inRoom && camPos == "bottom-icons";
            }
            else if (name == "WIRED_MENU")
            {
                // TODO(as3-port): IHabboUserDefinedRoomEvents.showToolbarMenuButton — not ported yet
                toggle.visible = false;
            }
        }

        CheckSize();
    }

    /// @see BottomBarLeft.as::calculateNewWidth
    public int CalculateNewWidth()
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

    /// @see BottomBarLeft.as::getToolbarAreaWidth
    public int GetToolbarAreaWidth()
    {
        if (_separator == null || _separator.parent == null)
        {
            return 0;
        }

        return _isCollapsed ? COLLAPSED_MARGIN : (int)(_separator.x + _separator.parent.x);
    }

    /// @see BottomBarLeft.as::isNewItemsNotificationEnabled
    public bool IsNewItemsNotificationEnabled()
    {
        return _toolbar.GetBoolean("toolbar.new_additions.notification.enabled");
    }

    /// @see BottomBarLeft.as::get memenu
    public MeMenuNewController? memenu => _meMenuController;

    /// @see BottomBarLeft.as::set unseenMiniMailMessageCount
    public int unseenMiniMailMessageCount
    {
        set => _unseenMiniMailCount = value;
    }

    /// @see BottomBarLeft.as::set unseenAchievementCount
    public int unseenAchievementCount
    {
        set => _unseenAchievementCount = value;
    }

    /// @see BottomBarLeft.as::set unseenForumsCount
    public int unseenForumsCount
    {
        set => _unseenForumsCount = value;
    }

    /// @see BottomBarLeft.as::get unseenMeMenuCount
    public int unseenMeMenuCount => _unseenMiniMailCount + _unseenAchievementCount + _unseenForumsCount;

    /// @see HabboToolbar.as::onCatalogEvent — show/hide new items label
    public void OnCatalogEvent(string eventType)
    {
        switch (eventType)
        {
            case "CATALOG_NEW_ITEMS_SHOW":
                if (_newItemsLabel != null && _newItemsNotificationEnabled)
                {
                    _newItemsLabel.visible = true;
                }
                break;
            case "CATALOG_NEW_ITEMS_HIDE":
                if (_newItemsLabel != null)
                {
                    _newItemsLabel.visible = false;
                }
                break;
        }
    }

    /// @see HabboToolbar.as::onWiredMenuEvent — update WIRED_MENU icon visibility
    public void OnWiredMenuEvent()
    {
        if (_window == null)
        {
            return;
        }

        string? wiredMenuName = HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_WIRED_MENU);
        IWindowContainer? wiredMenu = _window.FindChildByName(wiredMenuName!) as IWindowContainer;
        if (wiredMenu != null)
        {
            // TODO(as3-port): IHabboUserDefinedRoomEvents.showToolbarMenuButton — not ported yet; keep hidden
            wiredMenu.visible = false;
        }

        CheckSize();
    }

    // --- ILinkEventTracker ---

    /// @see BottomBarLeft.as::get linkPattern
    public string linkPattern => "toolbar/";

    /// @see BottomBarLeft.as::linkReceived
    public void LinkReceived(string param1)
    {
        string[] parts = param1.Split('/');
        if (parts.Length < 2)
        {
            return;
        }

        switch (parts[1])
        {
            case "memenu":
                _meMenuController?.ToggleVisibility();
                break;
            case "highlight":
                if (parts.Length <= 2)
                {
                    return;
                }

                switch (parts[2])
                {
                    case "catalog":
                        _windowManager.ShowHint(HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_CATALOGUE)!);
                        break;
                    case "navigator":
                        _windowManager.ShowHint(HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_NAVIGATOR)!);
                        break;
                    case "memenu":
                        _windowManager.ShowHint(HabboToolbarIconEnum.GetIconName(HabboToolbarIconEnum.ICON_MEMENU)!);
                        break;
                }
                break;
        }
    }

    // --- Private helpers ---

    /// @see BottomBarLeft.as::checkSize
    private void CheckSize()
    {
        if (_window == null || _windowManager == null)
        {
            return;
        }

        if (_leftArrow != null)
        {
            _leftArrow.visible = !_isCollapsed;
        }

        if (_rightArrow != null)
        {
            _rightArrow.visible = _isCollapsed;
        }

        // Godot adaptation: IWindow.desktop not available; get desktop from window manager
        IWindow? desktop = _windowManager.GetWindowContext(1)?.GetDesktopWindow();
        if (desktop != null)
        {
            _window.y = desktop.height - _window.height;
        }

        _window.width = (ICON_REGION_WIDTH * CalculateNewWidth()) + WINDOW_RIGHT_PADDING + TOOLBAR_EXTENSION_MARGIN;

        // @see BottomBarLeft.as::checkSize — reposition me menu when not collapsed
        if (!_isCollapsed)
        {
            _meMenuController?.Reposition();
        }

        _window.Invalidate();
    }

    /// @see BottomBarLeft.as::onParentResized
    private void OnParentResized(WindowEvent ev, IWindow window)
    {
        CheckSize();
    }

    /// @see BottomBarLeft.as::onIconClick
    private void OnIconClick(WindowEvent ev, IWindow window)
    {
        string name = window.name;
        if (!string.IsNullOrEmpty(name))
        {
            _toolbar.ToggleWindowVisibility(name);
            _windowManager.HideMatchingHint(name);
        }
    }

    /// @see BottomBarLeft.as::onCollapseToolsBar
    private void OnCollapseToolsBar(WindowEvent ev, IWindow window)
    {
        _isCollapsed = !_isCollapsed;
        SetToolbarState(_isCollapsed ? HabboToolbarEnum.TOOLBAR_STATE_COLLAPSED : _savedState);
        CheckSize();
        // TODO(as3-port): IHabboRoomUI.triggerbottomBarResize — not ported yet
    }

    /// @see BottomBarLeft.as::setIconHoverState
    private void SetIconHoverState(IWindow? iconBmp, string suffix)
    {
        if (iconBmp is IStaticBitmapWrapperWindow staticBmp)
        {
            staticBmp.AssetUri = iconBmp.name + suffix;
        }
        else if (iconBmp is IBitmapWrapperWindow bitmapWrap && iconBmp.name == ME_MENU_ICON_NAME)
        {
            bitmapWrap.Bitmap = _meMenuBitmap;
        }
    }

    /// @see BottomBarLeft.as::setIconBgHoverState
    private static void SetIconBgHoverState(IWindowContainer? iconBorder, string suffix)
    {
        if (iconBorder == null)
        {
            return;
        }

        iconBorder.color = suffix == ICON_MOUSE_OVER ? ICON_BG_COLOR_OVER : ICON_BG_COLOR_OUT;
    }

    /// @see BottomBarLeft.as::setMeMenuIconBitmaps
    private void SetMeMenuIconBitmaps(Image bitmap)
    {
        // Godot adaptation: Image is GC-managed; no explicit dispose/clone needed
        _meMenuBitmap = bitmap;
    }

    private void RegisterHintIfPresent(string iconName)
    {
        if (_window?.FindChildByName(iconName) is IWindowContainer container)
        {
            _windowManager.RegisterHintWindow(iconName, container);
        }
    }

    /// @see BottomBarLeft.as::getIconName (private — maps icon enum to icon-bitmap child name)
    public static string? GetIconChildName(int iconId)
    {
        return iconId switch
        {
            HabboToolbarIconEnum.ICON_CATALOGUE  => "icons_toolbar_catalogue",
            HabboToolbarIconEnum.ICON_INVENTORY  => "icons_toolbar_inventory",
            HabboToolbarIconEnum.ICON_MEMENU     => "MEMENU",
            HabboToolbarIconEnum.ICON_NAVIGATOR  => "icons_toolbar_navigator",
            HabboToolbarIconEnum.ICON_QUESTS     => "icons_toolbar_quests",
            HabboToolbarIconEnum.ICON_GAMES      => "icons_toolbar_games",
            HabboToolbarIconEnum.ICON_STORIES    => "icons_toolbar_stories",
            HabboToolbarIconEnum.ICON_RECEPTION  => "icons_toolbar_reception",
            HabboToolbarIconEnum.ICON_BUILDER    => "icons_toolbar_builder",
            HabboToolbarIconEnum.ICON_CAMERA     => "icons_toolbar_camera",
            HabboToolbarIconEnum.ICON_WIRED_MENU => "icons_toolbar_wired_menu",
            _                                    => null,
        };
    }

    private static string? GetStateVisibilityTag(int state)
    {
        return state switch
        {
            HabboToolbarEnum.TOOLBAR_STATE_GAME_CENTER_VIEW => "VISIBLE_GAME_CENTER",
            HabboToolbarEnum.TOOLBAR_STATE_HOTEL_VIEW       => "VISIBLE_HOTEL",
            HabboToolbarEnum.TOOLBAR_STATE_NOOB_NOT_HOME    => "VISIBLE_NOOB",
            HabboToolbarEnum.TOOLBAR_STATE_NOOB_HOME        => "VISIBLE_ROOM",
            HabboToolbarEnum.TOOLBAR_STATE_ROOM_VIEW        => "VISIBLE_ROOM",
            HabboToolbarEnum.TOOLBAR_STATE_COLLAPSED        => "VISIBLE_COLLAPSED",
            _                                               => null,
        };
    }
}
