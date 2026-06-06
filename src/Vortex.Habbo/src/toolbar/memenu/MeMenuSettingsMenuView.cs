// @see com.sulake.habbo.toolbar.memenu.MeMenuSettingsMenuView

using Vortex.Core.Window;
using Vortex.Core.Window.Events;
using Vortex.Habbo.Toolbar.MeMenu.ChatSettings;
using Vortex.Habbo.Toolbar.MeMenu.SoundSettings;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Toolbar.MeMenu;

/// @see com.sulake.habbo.toolbar.memenu.MeMenuSettingsMenuView
public class MeMenuSettingsMenuView
{
    private IWindowContainer? _window;
    private HabboToolbar? _toolbar;
    private MeMenuSoundSettingsView? _soundView;
    private MeMenuChatSettingsView? _chatView;

    // @see MeMenuSettingsMenuView.as — takes old MeMenuController + ToolbarView, duck-typed for new system too
    // @see MeMenuSettingsMenuView.as::MeMenuSettingsMenuView

    /// @see MeMenuSettingsMenuView.as::MeMenuSettingsMenuView
    public MeMenuSettingsMenuView(HabboToolbar toolbar)
    {
        _toolbar = toolbar;
        CreateWindow();
    }

    /// @see MeMenuSettingsMenuView.as::dispose
    public void Dispose()
    {
        _soundView?.Dispose();
        _soundView = null;
        _chatView?.Dispose();
        _chatView = null;

        if (_window != null)
        {
            _window.Destroy();
            _window = null;
        }

        _toolbar = null;
    }

    /// @see MeMenuSettingsMenuView.as::get window
    public IWindowContainer? window => _window;

    /// @see MeMenuSettingsMenuView.as::get disposed
    public bool disposed => _toolbar == null;

    private void CreateWindow()
    {
        if (_toolbar?.WindowManager == null)
        {
            return;
        }

        Core.Assets.XmlAsset? xmlAsset = (_toolbar.assets as Core.Assets.IAssetLibrary)?.GetAssetByName("me_menu_settings_menu_xml") as Core.Assets.XmlAsset;
        System.Xml.Linq.XElement? layoutXml = xmlAsset?.Content as System.Xml.Linq.XElement
            ?? HabboAssetResolver.LoadXmlAsset("me_menu_settings_menu_xml");

        if (layoutXml == null)
        {
            return;
        }

        _window = _toolbar.WindowManager.BuildFromXml(layoutXml, 2) as IWindowContainer;

        if (_window == null)
        {
            return;
        }

        for (int i = 0; i < _window.numChildren; i++)
        {
            _window.GetChildAt(i)?.AddEventListener(WindowMouseEvent.CLICK, OnButtonClicked);
        }

        _window.visible = false;
    }

    /// @see MeMenuSettingsMenuView.as::toggleVisibility
    public void ToggleVisibility()
    {
        if (_window == null)
        {
            return;
        }

        _window.visible = !_window.visible;

        if (_window.visible)
        {
            Reposition();
        }
    }

    /// @see MeMenuSettingsMenuView.as::reposition
    public void Reposition()
    {
        if (_window == null || _toolbar?.WindowManager == null)
        {
            return;
        }

        // @see MeMenuSettingsMenuView.as — position above the toolbar
        IWindow? toolbarWin = _toolbar.WindowManager.GetWindowContext(1)?.GetDesktopWindow();

        if (toolbarWin != null)
        {
            _window.x = 3;
            _window.y = toolbarWin.height - _window.height - 50;
        }
    }

    private void OnButtonClicked(WindowEvent ev, IWindow window)
    {
        switch (window.name)
        {
            case "back_btn":
            case "close_btn":
                ToggleVisibility();
                break;

            case "sound_btn":
                OpenSoundSettings();
                break;

            case "chat_btn":
                OpenChatSettings();
                break;

            case "other_btn":
                // @see MeMenuSettingsMenuView.as — other settings opens OtherSettingsView (reuses existing)
                if (_toolbar != null)
                {
                    Toolbar.Extensions.Settings.OtherSettingsView? otherView = new(_toolbar);
                    if (otherView.window != null && _window?.parent != null)
                    {
                        _window.parent.AddChild(otherView.window);
                        otherView.window.x = _window.x;
                        otherView.window.y = _window.y;
                    }
                }
                break;
        }
    }

    private void OpenSoundSettings()
    {
        _chatView?.Dispose();
        _chatView = null;
        _soundView?.Dispose();
        _soundView = null;

        if (_toolbar == null)
        {
            return;
        }

        _soundView = new MeMenuSoundSettingsView(_toolbar);

        if (_soundView.window != null && _window?.parent != null)
        {
            _window.parent.AddChild(_soundView.window);
            _soundView.window.x = _window.x;
            _soundView.window.y = _window.y;
        }
    }

    private void OpenChatSettings()
    {
        _soundView?.Dispose();
        _soundView = null;
        _chatView?.Dispose();
        _chatView = null;

        if (_toolbar == null)
        {
            return;
        }

        _chatView = new MeMenuChatSettingsView(_toolbar);

        if (_chatView.window != null && _window?.parent != null)
        {
            _window.parent.AddChild(_chatView.window);
            _chatView.window.x = _window.x;
            _chatView.window.y = _window.y;
        }
    }
}
