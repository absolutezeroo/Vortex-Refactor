// @see WIN63-202407091256-704579380-Source-main/habbo/toolbar/memenu/MeMenuNewController.as

using System;
using System.Xml.Linq;

using Vortex.Core.Assets;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Habbo.Toolbar.Events;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Toolbar.MeMenu;

/// @see WIN63-202407091256-704579380-Source-main/habbo/toolbar/memenu/MeMenuNewController.as
public class MeMenuNewController
{
    // @see MeMenuNewController.as::windowProcedure — hover text colours
    private const uint TEXT_COLOR_HOVER = 0x21D474;
    private const uint TEXT_COLOR_OUT = 0xFFFFFF;

    private HabboToolbar? _toolbar;
    private readonly BottomBarLeft _bottomBar;
    private IWindowContainer? _window;

    // @see MeMenuNewController.as — stored to allow RemoveEventListener on dispose
    private readonly Action<object?> _onToolbarClick;

    // TODO(as3-port): MeMenuNewIconLoader — requires IAvatarRenderManager (not ported)
    // TODO(as3-port): MeMenuSettingsMenuView — complex settings sub-menu, not ported yet
    // TODO(as3-port): _unseenItemCounters — requires IHabboWindowManager.createUnseenItemCounter (not ported)

    /// @see MeMenuNewController.as::MeMenuNewController
    public MeMenuNewController(HabboToolbar toolbar, BottomBarLeft bottomBar)
    {
        _toolbar = toolbar;
        _bottomBar = bottomBar;

        _onToolbarClick = OnToolbarClick;
        toolbar.events?.AddEventListener(HabboToolbarEvent.TOOLBAR_CLICK, _onToolbarClick);

        XmlAsset? xmlAsset = (toolbar.assets as IAssetLibrary)?.GetAssetByName("me_menu_new_view_xml") as XmlAsset;
        XElement? layoutXml = xmlAsset?.Content as XElement
            ?? HabboAssetResolver.LoadXmlAsset("me_menu_new_view_xml"); // Godot adaptation

        if (layoutXml == null)
        {
            throw new InvalidOperationException("MeMenuNewController: me_menu_new_view_xml not found");
        }

        _window = toolbar.WindowManager?.BuildFromXml(layoutXml, 2) as IWindowContainer;

        if (_window == null)
        {
            throw new InvalidOperationException("MeMenuNewController: failed to build me_menu_new_view window");
        }

        // @see MeMenuNewController.as — hide guide if guides not enabled
        if (!toolbar.GetBoolean("guides.enabled"))
        {
            SetGuideToolVisibility(false);
        }

        // @see MeMenuNewController.as — hide collectibles if not enabled
        if (!toolbar.GetBoolean("classic.collectibles.hub.enabled") || !toolbar.GetBoolean("collectibles.hub.enabled"))
        {
            SetCollectiblesVisibility(false);
        }

        SetMinimailVisibility(false);

        _window.visible = false;
        _window.procedure = WindowProcedure;
    }

    /// @see MeMenuNewController.as::get disposed
    public bool disposed => _toolbar == null;

    /// @see MeMenuNewController.as::get window
    public IWindowContainer? window => _window;

    // --- Unseen count setters ---

    /// @see MeMenuNewController.as::set unseenAchievementsCount
    public int unseenAchievementsCount
    {
        // TODO(as3-port): update counter badge once createUnseenItemCounter is ported
        set => _ = value;
    }

    /// @see MeMenuNewController.as::set unseenMinimailsCount
    public int unseenMinimailsCount
    {
        // TODO(as3-port): update counter badge once createUnseenItemCounter is ported
        set => _ = value;
    }

    /// @see MeMenuNewController.as::set unseenForumsCount
    public int unseenForumsCount
    {
        // TODO(as3-port): update counter badge once createUnseenItemCounter is ported
        set => _ = value;
    }

    /// @see MeMenuNewController.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        _toolbar!.events?.RemoveEventListener(HabboToolbarEvent.TOOLBAR_CLICK, _onToolbarClick);

        if (_window != null)
        {
            _window.Destroy();
            _window = null;
        }

        _toolbar = null;
    }

    /// @see MeMenuNewController.as::toggleVisibility
    public void ToggleVisibility()
    {
        if (_window == null || disposed)
        {
            return;
        }

        _window.visible = !_window.visible;

        if (_window.visible)
        {
            // @see MeMenuNewController.as — hide talents if talent track not enabled
            if (_toolbar != null && !_toolbar.GetBoolean("talent.track.enabled"))
            {
                IWindow? talents = _window.FindChildByName("talents");
                if (talents != null)
                {
                    talents.visible = false;
                }
            }

            // @see MeMenuNewController.as — guide visibility gated on perk
            // TODO(as3-port): ISessionDataManager.isPerkAllowed("USE_GUIDE_TOOL") — default false until ported
            if (_toolbar != null && _toolbar.GetBoolean("guides.enabled"))
            {
                SetGuideToolVisibility(false);
            }
        }

        Reposition();
    }

    /// @see MeMenuNewController.as::reposition
    public void Reposition()
    {
        if (_window == null || _bottomBar.window == null)
        {
            return;
        }

        // @see MeMenuNewController.as::reposition — attach to toolbar parent when first shown
        if (_window.parent == null && _bottomBar.window.parent != null)
        {
            _bottomBar.window.parent.AddChild(_window);
        }

        _window.x = 3;
        _window.y = _bottomBar.window.y - _window.height;
    }

    // --- Private helpers ---

    /// @see MeMenuNewController.as::windowProcedure — hover/click for menu item regions
    private void WindowProcedure(WindowEvent ev, IWindow window)
    {
        if (window is not IRegionWindow)
        {
            return;
        }

        string name = window.name ?? string.Empty;
        // @see MeMenuNewController.as::windowProcedure — icon_color and icon_grey are IWindow children
        IWindow? colorIcon = window.FindChildByName(name + "_icon_color");
        IWindow? greyIcon = window.FindChildByName(name + "_icon_grey");
        ITextWindow? textField = window.FindChildByName("field_text") as ITextWindow;

        switch (ev.type)
        {
            case WindowMouseEvent.OVER:
                if (colorIcon != null && greyIcon != null)
                {
                    colorIcon.visible = true;
                    greyIcon.visible = false;
                    if (textField != null)
                    {
                        textField.TextColor = TEXT_COLOR_HOVER;
                    }
                }
                break;

            case WindowMouseEvent.OUT:
                if (colorIcon != null && greyIcon != null)
                {
                    colorIcon.visible = false;
                    greyIcon.visible = true;
                    if (textField != null)
                    {
                        textField.TextColor = TEXT_COLOR_OUT;
                    }
                }
                break;

            case WindowMouseEvent.CLICK:
                if (_window != null)
                {
                    _window.visible = false;
                }
                HandleMenuClick(name);
                break;
        }
    }

    /// @see MeMenuNewController.as::windowProcedure — dispatch per-item click actions
    private void HandleMenuClick(string itemName)
    {
        if (_toolbar == null)
        {
            return;
        }

        switch (itemName)
        {
            case "profile":
                // TODO(as3-port): send class_322(sessionDataManager.userId) — ISessionDataManager not ported
                break;
            case "minimail":
                // TODO(as3-port): HabboWebTools.openMinimail — not ported
                break;
            case "rooms":
                // TODO(as3-port): IHabboNavigator.showOwnRooms — not ported
                break;
            case "talents":
                // TODO(as3-port): send class_1018(sessionDataManager.currentTalentTrack) — not ported
                break;
            case "settings":
                // @see MeMenuNewController.as — settings opens MeMenuSettingsMenuView; not ported yet
                break;
            case "achievements":
                // TODO(as3-port): IHabboQuestEngine.showAchievements — not ported
                break;
            case "guide":
                _toolbar.ToggleWindowVisibility("GUIDE");
                break;
            case "clothes":
                _toolbar.context?.CreateLinkEvent("avatareditor/open");
                break;
            case "forums":
                _toolbar.context?.CreateLinkEvent("groupforum/list/my");
                break;
            case "collectibles":
                _toolbar.context?.CreateLinkEvent("collectibles/open");
                break;
        }
    }

    /// @see MeMenuNewController.as — react to toolbar icon click events
    private void OnToolbarClick(object? param1)
    {
        if (param1 is not HabboToolbarEvent ev)
        {
            return;
        }

        if (ev.iconId == HabboToolbarIconEnum.ICON_MEMENU)
        {
            ToggleVisibility();
        }
        else if (_window != null)
        {
            _window.visible = false;
        }
    }

    /// @see MeMenuNewController.as::setGuideToolVisibility
    private void SetGuideToolVisibility(bool visible)
    {
        if (_window == null)
        {
            return;
        }

        IWindow? guide = _window.FindChildByName("guide");
        IWindow? achievements = _window.FindChildByName("achievements");

        if (guide != null)
        {
            guide.visible = visible;
        }

        // @see MeMenuNewController.as — resize window height to trim/show guide row
        if (achievements != null)
        {
            IWindow? anchor = visible ? guide : achievements;
            if (anchor != null)
            {
                _window.height = anchor.y + anchor.height + 5;
            }
        }
    }

    /// @see MeMenuNewController.as::setCollectiblesVisibility
    private void SetCollectiblesVisibility(bool visible)
    {
        IWindow? collectibles = _window?.FindChildByName("collectibles");
        if (collectibles != null)
        {
            collectibles.visible = visible;
        }
    }

    /// @see MeMenuNewController.as::setMinimailVisibility
    private void SetMinimailVisibility(bool visible)
    {
        IWindow? minimail = _window?.FindChildByName("minimail");
        if (minimail != null)
        {
            minimail.visible = visible;
        }
    }
}
