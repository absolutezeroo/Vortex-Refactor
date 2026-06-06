// @see com.sulake.habbo.toolbar.extensions.SettingsExtension

using Vortex.Core.Assets;
using Vortex.Core.Window;
using Vortex.Core.Window.Events;
using Vortex.Habbo.Toolbar.Extensions.Settings;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Toolbar.Extensions;

/// @see com.sulake.habbo.toolbar.extensions.SettingsExtension
public class SettingsExtension
{
    private const int SPACING = 3;
    private const int PADDING = 7;

    private HabboToolbar? _toolbar;
    private IWindowContainer? _window;
    private readonly List<IWindow> _buttons = new();
    private bool _disposed;

    /// @see SettingsExtension.as::SettingsExtension
    public SettingsExtension(HabboToolbar toolbar)
    {
        _toolbar = toolbar;

        XmlAsset? xmlAsset = (toolbar.assets as IAssetLibrary)?.GetAssetByName("settings_xml") as XmlAsset;
        System.Xml.Linq.XElement? layoutXml = xmlAsset?.Content as System.Xml.Linq.XElement
            ?? HabboAssetResolver.LoadXmlAsset("settings_xml");

        if (layoutXml == null || toolbar.WindowManager == null)
        {
            return;
        }

        _window = toolbar.WindowManager.BuildFromXml(layoutXml) as IWindowContainer;

        if (_window == null)
        {
            return;
        }

        _window.procedure = WindowProcedure;

        // TODO(as3-port): ILocalization.getLocalization — use key fallbacks until localization is ported
        AddButton("avatar_settings", "avatar settings");
        AddButton("sound", "sound settings");
        AddButton("chat", "other settings");

        toolbar.extensionView?.AttachExtension(ToolbarDisplayExtensionIds.const_673, _window, ExtensionFixedSlotsEnum.SLOT_SETTINGS);

        _window.visible = false;
    }

    /// @see SettingsExtension.as::get window
    public IWindowContainer? window => _window;

    /// @see SettingsExtension.as::dispose
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _toolbar = null;
        _disposed = true;
    }

    /// @see SettingsExtension.as::get disposed
    public bool disposed => _disposed;

    private void AddButton(string name, string label)
    {
        if (_toolbar?.WindowManager == null)
        {
            return;
        }

        XmlAsset? btnXmlAsset = (_toolbar.assets as IAssetLibrary)?.GetAssetByName("setting_category_xml") as XmlAsset;
        System.Xml.Linq.XElement? btnXml = btnXmlAsset?.Content as System.Xml.Linq.XElement
            ?? HabboAssetResolver.LoadXmlAsset("setting_category_xml");

        if (btnXml == null)
        {
            return;
        }

        IWindowContainer? btn = _toolbar.WindowManager.BuildFromXml(btnXml) as IWindowContainer;

        if (btn == null)
        {
            return;
        }

        _window?.AddChild(btn);

        IWindow? labelWin = btn.FindChildByName("button_label");
        if (labelWin != null)
        {
            labelWin.caption = label;
        }

        if (_buttons.Count > 0)
        {
            btn.y = _buttons[^1].y + _buttons[^1].height + SPACING;
        }
        else
        {
            btn.y = PADDING;
        }

        btn.x = PADDING;
        btn.name = name;
        btn.procedure = WindowProcedure;
        _buttons.Add(btn);

        if (_window != null && _buttons.Count > 0)
        {
            _window.height = (int)(_buttons[^1].y + _buttons[^1].height + PADDING);
        }
    }

    private void WindowProcedure(WindowEvent ev, IWindow window)
    {
        if (ev.type != WindowMouseEvent.CLICK)
        {
            return;
        }

        switch (window.name)
        {
            case "avatar_settings":
                // TODO(as3-port): HabboWebTools.openAvatars() — not ported
                // TODO(as3-port): toolbar.toggleSettingVisibility() — not ported
                break;
            case "sound":
                OpenSoundSettingsWindow();
                // TODO(as3-port): toolbar.toggleSettingVisibility() — not ported
                break;
            case "chat":
                OpenChatSettingsWindow();
                break;
        }
    }

    private void OpenSoundSettingsWindow()
    {
        if (_toolbar?.WindowManager == null)
        {
            return;
        }

        SoundSettingsView view = new(_toolbar);
        IWindowContainer? desktop = _toolbar.WindowManager.GetWindowContext(1)?.GetDesktopWindow() as IWindowContainer;

        if (desktop != null && view.window != null)
        {
            desktop.AddChild(view.window);
            view.window.x = desktop.width - view.window.width - 200;
        }
    }

    private void OpenChatSettingsWindow()
    {
        if (_toolbar?.WindowManager == null)
        {
            return;
        }

        OtherSettingsView view = new(_toolbar);
        IWindowContainer? desktop = _toolbar.WindowManager.GetWindowContext(1)?.GetDesktopWindow() as IWindowContainer;

        if (desktop != null && view.window != null)
        {
            desktop.AddChild(view.window);
            view.window.x = desktop.width - view.window.width - 200;
        }
    }
}
