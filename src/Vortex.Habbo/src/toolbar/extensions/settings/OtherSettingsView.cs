// @see com.sulake.habbo.toolbar.extensions.settings.OtherSettingsView

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Toolbar.Extensions.Settings;

/// @see com.sulake.habbo.toolbar.extensions.settings.OtherSettingsView
public class OtherSettingsView
{
    private IWindowContainer? _window;
    private HabboToolbar? _toolbar;

    /// @see OtherSettingsView.as::OtherSettingsView
    public OtherSettingsView(HabboToolbar toolbar)
    {
        _toolbar = toolbar;
        CreateWindow();
    }

    /// @see OtherSettingsView.as::dispose
    public void Dispose()
    {
        if (_window == null)
        {
            return;
        }

        _window.Destroy();
        _window = null;
    }

    /// @see OtherSettingsView.as::get window
    public IWindowContainer? window => _window;

    private void CreateWindow()
    {
        if (_toolbar?.WindowManager == null)
        {
            return;
        }

        var xmlAsset = (_toolbar.assets as Core.Assets.IAssetLibrary)?.GetAssetByName("me_menu_other_settings_xml") as Core.Assets.XmlAsset;
        System.Xml.Linq.XElement? layoutXml = xmlAsset?.Content as System.Xml.Linq.XElement
            ?? HabboAssetResolver.LoadXmlAsset("me_menu_other_settings_xml");

        if (layoutXml == null)
        {
            return;
        }

        _window = _toolbar.WindowManager.BuildFromXml(layoutXml) as IWindowContainer;

        if (_window == null)
        {
            return;
        }

        for (int i = 0; i < _window.numChildren; i++)
        {
            _window.GetChildAt(i)?.AddEventListener(WindowMouseEvent.CLICK, OnButtonClicked);
        }

        // TODO(as3-port): IHabboFreeFlowChat.isDisabledInPreferences — not ported; skip checkbox init
        // TODO(as3-port): IHabboMessenger.getRoomInvitesIgnored() — not ported; skip checkbox init

        // @see OtherSettingsView.as — camera follow checkbox visible only when "room.camera.follow_user" is true
        IWindow? cameraCheckbox = _window.FindChildByName("disable_room_camera_follow_checkbox");
        IWindow? cameraLabel = _window.FindChildByName("disable_room_camera_follow_label");
        bool cameraFollowEnabled = _toolbar.GetBoolean("room.camera.follow_user");
        if (cameraCheckbox != null)
        {
            cameraCheckbox.visible = cameraFollowEnabled;
        }

        if (cameraLabel != null)
        {
            cameraLabel.visible = cameraFollowEnabled;
        }

        // @see OtherSettingsView.as — init checkbox state from session
        if (cameraCheckbox is IClass3398 checkboxWindow)
        {
            checkboxWindow.IsSelected = _toolbar.sessionDataManager?.isRoomCameraFollowDisabled ?? false;
        }

        // @see OtherSettingsView.as — phone number reset button logic
        bool smsEnabled = _toolbar.GetBoolean("sms.identity.verification.enabled");
        bool phoneVerified = _toolbar.GetInteger("phone.verification.status", 0) == 2;
        bool phoneCollected = _toolbar.GetInteger("phone.collection.status", 0) == 2;
        bool smsButtonEnabled = _toolbar.GetBoolean("sms.identity.verification.button.enabled");
        bool phoneCollectionZero = _toolbar.GetInteger("phone.collection.status", 0) == 0;
        bool showPhoneReset = smsEnabled && !phoneVerified && (phoneCollected || (smsButtonEnabled && phoneCollectionZero));
        IWindow? phoneResetBtn = _window.FindChildByName("btn_reset_phone_number_collection");
        if (phoneResetBtn != null)
        {
            phoneResetBtn.visible = showPhoneReset;
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
                // TODO(as3-port): send class_542 + ISessionDataManager.setRoomCameraFollowDisabled — not ported
                break;
            case "btn_reset_phone_number_collection":
                if (_window != null)
                {
                    IWindow? btn = _window.FindChildByName("btn_reset_phone_number_collection");
                    if (btn != null)
                    {
                        btn.visible = false;
                    }
                }
                // TODO(as3-port): send class_706 (phone number reset composer) — not ported
                break;
        }
    }
}
