// @see com.sulake.habbo.toolbar.memenu.soundsettings.MeMenuSoundSettingsItem

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;

namespace Vortex.Habbo.Toolbar.MeMenu.SoundSettings;

/// @see com.sulake.habbo.toolbar.memenu.soundsettings.MeMenuSoundSettingsItem
public class MeMenuSoundSettingsItem
{
    public const int TYPE_UI_VOLUME = 0;
    public const int TYPE_FURNI_VOLUME = 1;
    public const int TYPE_TRAX_VOLUME = 2;

    private MeMenuSoundSettingsView? _soundView;
    private readonly int _type;
    private IWindowContainer? _container;
    private MeMenuSoundSettingsSlider? _slider;
    private ITextWindow? _valueLabel;

    /// @see MeMenuSoundSettingsItem.as::MeMenuSoundSettingsItem
    public MeMenuSoundSettingsItem(MeMenuSoundSettingsView soundView, int type, IWindowContainer container)
    {
        _soundView = soundView;
        _type = type;
        _container = container;

        IWindowContainer? sliderContainer = container.FindChildByName("slider_container") as IWindowContainer;

        if (sliderContainer != null)
        {
            _slider = new MeMenuSoundSettingsSlider(sliderContainer);
            _slider.ValueChanged += OnSliderChanged;
        }

        _valueLabel = container.FindChildByName("value_label") as ITextWindow;

        IWindow? muteBtn = container.FindChildByName("mute_btn");
        muteBtn?.AddEventListener(WindowMouseEvent.CLICK, OnMuteClicked);
    }

    /// @see MeMenuSoundSettingsItem.as::dispose
    public void Dispose()
    {
        _slider?.Dispose();
        _slider = null;
        _soundView = null;
        _container = null;
    }

    /// @see MeMenuSoundSettingsItem.as::setValue
    public void SetValue(float value)
    {
        _slider?.SetValue(value);
        UpdateLabel(value);
    }

    private void UpdateLabel(float value)
    {
        if (_valueLabel != null)
        {
            _valueLabel.caption = $"{(int)(value * 100)}%";
        }
    }

    private void OnSliderChanged(float newValue)
    {
        UpdateLabel(newValue);
        _soundView?.OnItemChanged(_type, newValue);
    }

    private void OnMuteClicked(WindowEvent ev, IWindow window)
    {
        float current = _slider?.value ?? 0f;
        float next = current > 0f ? 0f : 1f;
        _slider?.SetValue(next);
        UpdateLabel(next);
        _soundView?.OnItemChanged(_type, next);
    }
}
