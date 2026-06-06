// @see com.sulake.habbo.toolbar.extensions.settings.SoundSettingsView

using Vortex.Core.Window;
using Vortex.Core.Window.Events;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Toolbar.Extensions.Settings;

/// @see com.sulake.habbo.toolbar.extensions.settings.SoundSettingsView
public class SoundSettingsView
{
    private IWindowContainer? _window;
    private SoundSettingsItem? _uiVolumeItem;
    private SoundSettingsItem? _furniVolumeItem;
    private SoundSettingsItem? _traxVolumeItem;
    private HabboToolbar? _toolbar;

    private float _genericVolume = 1f;
    private float _furniVolume = 1f;
    private float _traxVolume = 1f;

    // TODO(as3-port): ISoundManager — not ported yet

    /// @see SoundSettingsView.as::SoundSettingsView
    public SoundSettingsView(HabboToolbar toolbar)
    {
        _toolbar = toolbar;
        CreateWindow();
    }

    /// @see SoundSettingsView.as::dispose
    public void Dispose()
    {
        SaveVolume(_genericVolume, _furniVolume, _traxVolume);

        if (_window != null)
        {
            _window.Destroy();
            _window = null;
        }

        _uiVolumeItem?.Dispose();
        _uiVolumeItem = null;
        _furniVolumeItem?.Dispose();
        _furniVolumeItem = null;
        _traxVolumeItem?.Dispose();
        _traxVolumeItem = null;
    }

    /// @see SoundSettingsView.as::get window
    public IWindowContainer? window => _window;

    /// @see SoundSettingsView.as::updateSettings
    public void UpdateSettings()
    {
        // TODO(as3-port): read from ISoundManager.genericVolume/furniVolume/traxVolume — not ported
        _uiVolumeItem?.SetValue(_genericVolume);
        _furniVolumeItem?.SetValue(_furniVolume);
        _traxVolumeItem?.SetValue(_traxVolume);
    }

    /// @see SoundSettingsView.as::saveVolume
    public void SaveVolume(float genericVolume, float furniVolume, float traxVolume, bool apply = true)
    {
        float finalFurni = furniVolume >= 0 ? furniVolume : _furniVolume;
        float finalGeneric = genericVolume >= 0 ? genericVolume : _genericVolume;
        float finalTrax = traxVolume >= 0 ? traxVolume : _traxVolume;

        if (apply)
        {
            if (_toolbar == null)
            {
                return;
            }

            // TODO(as3-port): ISoundManager.furniVolume/genericVolume/traxVolume — not ported
            _furniVolume = finalFurni;
            _genericVolume = finalGeneric;
            _traxVolume = finalTrax;
        }
        else
        {
            // TODO(as3-port): ISoundManager.previewVolume — not ported
        }
    }

    /// @see SoundSettingsView.as::get toolbar
    public HabboToolbar? toolbar => _toolbar;

    private void CreateWindow()
    {
        if (_toolbar?.WindowManager == null)
        {
            return;
        }

        var xmlAsset = (_toolbar.assets as Core.Assets.IAssetLibrary)?.GetAssetByName("me_menu_sound_settings_xml") as Core.Assets.XmlAsset;
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

        for (int i = 0; i < _window.numChildren; i++)
        {
            _window.GetChildAt(i)?.AddEventListener(WindowMouseEvent.CLICK, OnButtonClicked);
        }

        IWindowContainer? uiContainer = _window.FindChildByName("ui_volume_container") as IWindowContainer;
        IWindowContainer? furniContainer = _window.FindChildByName("furni_volume_container") as IWindowContainer;
        IWindowContainer? traxContainer = _window.FindChildByName("trax_volume_container") as IWindowContainer;

        if (uiContainer != null)
        {
            _uiVolumeItem = new SoundSettingsItem(this, SoundSettingsItem.TYPE_UI_VOLUME, uiContainer);
        }

        if (furniContainer != null)
        {
            _furniVolumeItem = new SoundSettingsItem(this, SoundSettingsItem.TYPE_FURNI_VOLUME, furniContainer);
        }

        if (traxContainer != null)
        {
            _traxVolumeItem = new SoundSettingsItem(this, SoundSettingsItem.TYPE_TRAX_VOLUME, traxContainer);
        }

        UpdateSettings();
    }

    private void OnButtonClicked(WindowEvent ev, IWindow window)
    {
        if (window.name == "back_btn")
        {
            Dispose();
        }
    }
}
