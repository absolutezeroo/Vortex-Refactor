// @see com.sulake.habbo.toolbar.extensions.settings.SoundSettingsItem

using System;

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;

namespace Vortex.Habbo.Toolbar.Extensions.Settings;

/// @see com.sulake.habbo.toolbar.extensions.settings.SoundSettingsItem
public class SoundSettingsItem
{
    public const int TYPE_UI_VOLUME = 0;
    public const int TYPE_FURNI_VOLUME = 1;
    public const int TYPE_TRAX_VOLUME = 2;

    private SoundSettingsView? _soundView;
    private readonly int _type;
    private IWindowContainer? _container;
    private IWindow? _minusBtn;
    private IWindow? _plusBtn;
    private float _value;

    /// @see SoundSettingsItem.as::SoundSettingsItem
    public SoundSettingsItem(SoundSettingsView soundView, int type, IWindowContainer container)
    {
        _soundView = soundView;
        _type = type;
        _container = container;

        _minusBtn = container.FindChildByName("minus_btn");
        _plusBtn = container.FindChildByName("plus_btn");

        _minusBtn?.AddEventListener(WindowMouseEvent.CLICK, OnMinusClicked);
        _plusBtn?.AddEventListener(WindowMouseEvent.CLICK, OnPlusClicked);
    }

    /// @see SoundSettingsItem.as::dispose
    public void Dispose()
    {
        _soundView = null;
        _minusBtn?.RemoveEventListener(WindowMouseEvent.CLICK, OnMinusClicked);
        _plusBtn?.RemoveEventListener(WindowMouseEvent.CLICK, OnPlusClicked);
        _container = null;
    }

    /// @see SoundSettingsItem.as::setValue
    public void SetValue(float value)
    {
        _value = Math.Clamp(value, 0f, 1f);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        // @see SoundSettingsItem.as — visual bar fill, label, and button state
        IWindow? bar = _container?.FindChildByName("volume_bar");
        ITextWindow? label = _container?.FindChildByName("volume_label") as ITextWindow;

        if (bar != null)
        {
            // TODO(as3-port): set bar fill width based on _value — requires bitmap/fill API
        }

        if (label != null)
        {
            label.caption = $"{(int)(_value * 100)}%";
        }

        if (_minusBtn != null)
        {
            _minusBtn.visible = _value > 0f;
        }

        if (_plusBtn != null)
        {
            _plusBtn.visible = _value < 1f;
        }
    }

    private void OnMinusClicked(WindowEvent ev, IWindow window)
    {
        float newVal = Math.Max(0f, _value - 0.1f);
        _soundView?.SaveVolume(
            _type == TYPE_UI_VOLUME ? newVal : -1f,
            _type == TYPE_FURNI_VOLUME ? newVal : -1f,
            _type == TYPE_TRAX_VOLUME ? newVal : -1f,
            false);
        SetValue(newVal);
    }

    private void OnPlusClicked(WindowEvent ev, IWindow window)
    {
        float newVal = Math.Min(1f, _value + 0.1f);
        _soundView?.SaveVolume(
            _type == TYPE_UI_VOLUME ? newVal : -1f,
            _type == TYPE_FURNI_VOLUME ? newVal : -1f,
            _type == TYPE_TRAX_VOLUME ? newVal : -1f,
            false);
        SetValue(newVal);
    }
}
