// @see com.sulake.habbo.toolbar.memenu.chatsettings.MeMenuChatSettingsView

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Toolbar.MeMenu.ChatSettings;

/// @see com.sulake.habbo.toolbar.memenu.chatsettings.MeMenuChatSettingsView
public class MeMenuChatSettingsView
{
    private IWindowContainer? _window;
    private HabboToolbar? _toolbar;

    /// @see MeMenuChatSettingsView.as::MeMenuChatSettingsView
    public MeMenuChatSettingsView(HabboToolbar toolbar)
    {
        _toolbar = toolbar;
        CreateWindow();
    }

    /// @see MeMenuChatSettingsView.as::dispose
    public void Dispose()
    {
        if (_window == null)
        {
            return;
        }

        _window.Destroy();
        _window = null;
        _toolbar = null;
    }

    /// @see MeMenuChatSettingsView.as::get window
    public IWindowContainer? window => _window;

    private void CreateWindow()
    {
        if (_toolbar?.WindowManager == null)
        {
            return;
        }

        Core.Assets.XmlAsset? xmlAsset = (_toolbar.assets as Core.Assets.IAssetLibrary)?.GetAssetByName("me_menu_chat_settings_xml") as Core.Assets.XmlAsset;
        System.Xml.Linq.XElement? layoutXml = xmlAsset?.Content as System.Xml.Linq.XElement
            ?? HabboAssetResolver.LoadXmlAsset("me_menu_chat_settings_xml");

        if (layoutXml == null)
        {
            return;
        }

        _window = _toolbar.WindowManager.BuildFromXml(layoutXml) as IWindowContainer;

        if (_window == null)
        {
            return;
        }

        // @see MeMenuChatSettingsView.as — init prefer_old_chat checkbox from free flow chat preference
        // TODO(as3-port): IHabboFreeFlowChat.isDisabledInPreferences — not ported; skip init

        // @see MeMenuChatSettingsView.as — init ignore_room_invites checkbox from messenger
        // TODO(as3-port): IHabboMessenger.getRoomInvitesIgnored — not ported; skip init

        // @see MeMenuChatSettingsView.as — camera follow checkbox
        bool cameraFollowEnabled = _toolbar.GetBoolean("room.camera.follow_user");
        IWindow? cameraCheckbox = _window.FindChildByName("disable_room_camera_follow_checkbox");
        IWindow? cameraLabel = _window.FindChildByName("disable_room_camera_follow_label");
        if (cameraCheckbox != null)
        {
            cameraCheckbox.visible = cameraFollowEnabled;
        }

        if (cameraLabel != null)
        {
            cameraLabel.visible = cameraFollowEnabled;
        }

        if (cameraCheckbox is IClass3398 checkboxWindow)
        {
            checkboxWindow.IsSelected = _toolbar.sessionDataManager?.isRoomCameraFollowDisabled ?? false;
        }

        for (int i = 0; i < _window.numChildren; i++)
        {
            _window.GetChildAt(i)?.AddEventListener(WindowMouseEvent.CLICK, OnButtonClicked);
        }
    }

    private void OnButtonClicked(WindowEvent ev, IWindow window)
    {
        switch (window.name)
        {
            case "back_btn":
                Dispose();
                break;
            case "prefer_old_chat_checkbox":
                // TODO(as3-port): IHabboFreeFlowChat.isDisabledInPreferences toggle — not ported
                break;
            case "ignore_room_invites_checkbox":
                // TODO(as3-port): IHabboMessenger.setRoomInvitesIgnored + send class_856 — not ported
                break;
            case "disable_room_camera_follow_checkbox":
                if (_toolbar?.sessionDataManager != null)
                {
                    bool nowDisabled = !_toolbar.sessionDataManager.isRoomCameraFollowDisabled;
                    _toolbar.sessionDataManager.SetRoomCameraFollowDisabled(nowDisabled);
                }
                // TODO(as3-port): send class_542 (camera follow disabled composer) — not ported
                break;
        }
    }
}
