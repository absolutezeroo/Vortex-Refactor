// @see com.sulake.habbo.toolbar.memenu.MeMenuController

using System;
using System.Xml.Linq;

using Vortex.Core.Assets;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Habbo.Toolbar.Events;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Toolbar.MeMenu;

/// @see com.sulake.habbo.toolbar.memenu.MeMenuController (legacy toolbar system)
public class MeMenuController
{
    private const uint TEXT_COLOR_HOVER = 0x21D474;
    private const uint TEXT_COLOR_OUT = 0xFFFFFF;

    private HabboToolbar? _toolbar;
    private ToolbarView? _toolbarView;
    private IWindowContainer? _window;
    private MeMenuIconLoader? _iconLoader;
    private MeMenuSettingsMenuView? _settingsMenuView;

    private readonly Action<object?> _onToolbarClick;

    /// @see MeMenuController.as::MeMenuController
    public MeMenuController(HabboToolbar toolbar, ToolbarView toolbarView)
    {
        _toolbar = toolbar;
        _toolbarView = toolbarView;

        _iconLoader = new MeMenuIconLoader(toolbar);

        _onToolbarClick = OnToolbarClick;
        toolbar.events?.AddEventListener(HabboToolbarEvent.TOOLBAR_CLICK, _onToolbarClick);

        XmlAsset? xmlAsset = (toolbar.assets as IAssetLibrary)?.GetAssetByName("me_menu_view_xml") as XmlAsset;
        XElement? layoutXml = xmlAsset?.Content as XElement
            ?? HabboAssetResolver.LoadXmlAsset("me_menu_view_xml");

        if (layoutXml == null)
        {
            throw new InvalidOperationException("MeMenuController: me_menu_view_xml not found");
        }

        _window = toolbar.WindowManager?.BuildFromXml(layoutXml, 2) as IWindowContainer;

        if (_window == null)
        {
            throw new InvalidOperationException("MeMenuController: failed to build me_menu_view window");
        }

        _window.visible = false;
        _window.procedure = WindowProcedure;
    }

    /// @see MeMenuController.as::get disposed
    public bool disposed => _toolbar == null;

    /// @see MeMenuController.as::get window
    public IWindowContainer? window => _window;

    /// @see MeMenuController.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        _toolbar!.events?.RemoveEventListener(HabboToolbarEvent.TOOLBAR_CLICK, _onToolbarClick);

        _iconLoader?.Dispose();
        _iconLoader = null;

        _settingsMenuView?.Dispose();
        _settingsMenuView = null;

        if (_window != null)
        {
            _window.Destroy();
            _window = null;
        }

        _toolbar = null;
        _toolbarView = null;
    }

    /// @see MeMenuController.as::toggleVisibility
    public void ToggleVisibility()
    {
        if (_window == null || disposed)
        {
            return;
        }

        _window.visible = !_window.visible;

        if (_window.visible)
        {
            // @see MeMenuController.as — guide visible only when perk allowed
            if (_toolbar != null && _toolbar.GetBoolean("guides.enabled"))
            {
                bool guideAllowed = _toolbar.sessionDataManager?.IsPerkAllowed("USE_GUIDE_TOOL") ?? false;
                SetGuideToolVisibility(guideAllowed);
            }

            Reposition();
        }
    }

    /// @see MeMenuController.as::reposition
    public void Reposition()
    {
        if (_window == null || _toolbarView?.window == null)
        {
            return;
        }

        if (_window.parent == null && _toolbarView.window.parent != null)
        {
            _toolbarView.window.parent.AddChild(_window);
        }

        _window.x = 3;
        _window.y = _toolbarView.window.y - _window.height;
    }

    private void WindowProcedure(WindowEvent ev, IWindow window)
    {
        if (window is not IRegionWindow)
        {
            return;
        }

        string name = window.name ?? string.Empty;
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

    private void HandleMenuClick(string itemName)
    {
        if (_toolbar == null)
        {
            return;
        }

        switch (itemName)
        {
            case "profile":
                if (_toolbar.sessionDataManager != null)
                {
                    // TODO(as3-port): send class_322(sessionDataManager.userId) — composer not ported
                    _ = _toolbar.sessionDataManager.userId;
                }
                break;
            case "rooms":
                // TODO(as3-port): IHabboNavigator.showOwnRooms — not ported
                break;
            case "talents":
                if (_toolbar.sessionDataManager != null)
                {
                    // TODO(as3-port): send class_1018(currentTalentTrack) — composer not ported
                    _ = _toolbar.sessionDataManager.currentTalentTrack;
                }
                break;
            case "settings":
                if (_settingsMenuView == null)
                {
                    _settingsMenuView = new MeMenuSettingsMenuView(_toolbar);
                }
                _settingsMenuView.ToggleVisibility();
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
        }
    }

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

        if (achievements != null)
        {
            IWindow? anchor = visible ? guide : achievements;
            if (anchor != null)
            {
                _window.height = anchor.y + anchor.height + 5;
            }
        }
    }
}
