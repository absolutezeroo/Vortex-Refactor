// @see com.sulake.habbo.toolbar.memenu.soundsettings.MeMenuSoundSettingsView

using Vortex.Core.Window;
using Vortex.Core.Window.Events;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Toolbar.MeMenu.SoundSettings;

/// @see com.sulake.habbo.toolbar.memenu.soundsettings.MeMenuSoundSettingsView
public class MeMenuSoundSettingsView
{
    private IWindowContainer? _window;
    private HabboToolbar? _toolbar;
    private MeMenuSoundSettingsItem? _uiVolumeItem;
    private MeMenuSoundSettingsItem? _furniVolumeItem;
    private MeMenuSoundSettingsItem? _traxVolumeItem;

    private float _genericVolume = 1f;
    private float _furniVolume = 1f;
    private float _traxVolume = 1f;

    // TODO(as3-port): ISoundManager — not ported yet

    /// @see MeMenuSoundSettingsView.as::MeMenuSoundSettingsView
    public MeMenuSoundSettingsView(HabboToolbar toolbar)
    {
        _toolbar = toolbar;
        CreateWindow();
    }

    /// @see MeMenuSoundSettingsView.as::dispose
    public void Dispose()
    {
        SaveVolumes();

        _uiVolumeItem?.Dispose();
        _uiVolumeItem = null;
        _furniVolumeItem?.Dispose();
        _furniVolumeItem = null;
        _traxVolumeItem?.Dispose();
        _traxVolumeItem = null;

        if (_window != null)
        {
            _window.Destroy();
            _window = null;
        }

        _toolbar = null;
    }

    /// @see MeMenuSoundSettingsView.as::get window
    public IWindowContainer? window => _window;

    /// @see MeMenuSoundSettingsView.as::updateSettings
    public void UpdateSettings()
    {
        // TODO(as3-port): read from ISoundManager.genericVolume/furniVolume/traxVolume
        _uiVolumeItem?.SetValue(_genericVolume);
        _furniVolumeItem?.SetValue(_furniVolume);
        _traxVolumeItem?.SetValue(_traxVolume);
    }

    /// @see MeMenuSoundSettingsView — called by items when slider changes
    public void OnItemChanged(int type, float newValue)
    {
        switch (type)
        {
            case MeMenuSoundSettingsItem.TYPE_UI_VOLUME:
                _genericVolume = newValue;
                break;
            case MeMenuSoundSettingsItem.TYPE_FURNI_VOLUME:
                _furniVolume = newValue;
                break;
            case MeMenuSoundSettingsItem.TYPE_TRAX_VOLUME:
                _traxVolume = newValue;
                break;
        }

        // TODO(as3-port): apply preview volume via ISoundManager.previewVolume — not ported
    }

    private void SaveVolumes()
    {
        // TODO(as3-port): persist to ISoundManager.genericVolume/furniVolume/traxVolume — not ported
    }

    private void CreateWindow()
    {
        if (_toolbar?.WindowManager == null)
        {
            return;
        }

        Core.Assets.XmlAsset? xmlAsset = (_toolbar.assets as Core.Assets.IAssetLibrary)?.GetAssetByName("me_menu_sound_settings_xml") as Core.Assets.XmlAsset;
        System.Xml.Linq.XElement? layoutXml = xmlAsset?.Content as System.Xml.Linq.XElement
            ?? HabboAssetResolver.LoadXmlAsset("me_menu_sound_settings_xml");

        if (layoutXml == null)
        {
            return;
        }

        _window = _toolbar.WindowManager.BuildFromXml(layoutXml) as IWindowContainer;

        if (_window == null)
        {
            return;
        }

        IWindowContainer? uiContainer = _window.FindChildByName("ui_volume_container") as IWindowContainer;
        IWindowContainer? furniContainer = _window.FindChildByName("furni_volume_container") as IWindowContainer;
        IWindowContainer? traxContainer = _window.FindChildByName("trax_volume_container") as IWindowContainer;

        if (uiContainer != null)
        {
            _uiVolumeItem = new MeMenuSoundSettingsItem(this, MeMenuSoundSettingsItem.TYPE_UI_VOLUME, uiContainer);
        }

        if (furniContainer != null)
        {
            _furniVolumeItem = new MeMenuSoundSettingsItem(this, MeMenuSoundSettingsItem.TYPE_FURNI_VOLUME, furniContainer);
        }

        if (traxContainer != null)
        {
            _traxVolumeItem = new MeMenuSoundSettingsItem(this, MeMenuSoundSettingsItem.TYPE_TRAX_VOLUME, traxContainer);
        }

        IWindow? backBtn = _window.FindChildByName("back_btn");
        backBtn?.AddEventListener(WindowMouseEvent.CLICK, OnBackClicked);

        UpdateSettings();
    }

    private void OnBackClicked(WindowEvent ev, IWindow window)
    {
        Dispose();
    }
}
