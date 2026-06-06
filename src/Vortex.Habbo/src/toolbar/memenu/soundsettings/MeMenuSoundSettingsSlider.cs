// @see com.sulake.habbo.toolbar.memenu.soundsettings.MeMenuSoundSettingsSlider

using System;

using Vortex.Core.Window;
using Vortex.Core.Window.Events;

namespace Vortex.Habbo.Toolbar.MeMenu.SoundSettings;

/// @see com.sulake.habbo.toolbar.memenu.soundsettings.MeMenuSoundSettingsSlider
public class MeMenuSoundSettingsSlider
{
    private const int SLIDER_WIDTH = 80;

    private IWindowContainer? _container;
    private IWindow? _handle;
    private IWindow? _track;
    private float _value;
    private bool _dragging;

    public event Action<float>? ValueChanged;

    /// @see MeMenuSoundSettingsSlider.as::MeMenuSoundSettingsSlider
    public MeMenuSoundSettingsSlider(IWindowContainer container)
    {
        _container = container;
        _handle = container.FindChildByName("slider_handle");
        _track = container.FindChildByName("slider_track");

        _handle?.AddEventListener(WindowMouseEvent.DOWN, OnMouseDown);
        container.AddEventListener(WindowMouseEvent.UP, OnMouseUp);
        container.AddEventListener(WindowMouseEvent.MOVE, OnMouseMove);
        container.AddEventListener(WindowMouseEvent.CLICK, OnTrackClick);
    }

    /// @see MeMenuSoundSettingsSlider.as::dispose
    public void Dispose()
    {
        _handle?.RemoveEventListener(WindowMouseEvent.DOWN, OnMouseDown);
        _container?.RemoveEventListener(WindowMouseEvent.UP, OnMouseUp);
        _container?.RemoveEventListener(WindowMouseEvent.MOVE, OnMouseMove);
        _container?.RemoveEventListener(WindowMouseEvent.CLICK, OnTrackClick);
        _container = null;
        _handle = null;
        _track = null;
    }

    /// @see MeMenuSoundSettingsSlider.as::get value
    public float value => _value;

    /// @see MeMenuSoundSettingsSlider.as::setValue
    public void SetValue(float v)
    {
        _value = Math.Clamp(v, 0f, 1f);
        UpdateHandlePosition();
    }

    private void UpdateHandlePosition()
    {
        if (_handle == null || _track == null)
        {
            return;
        }

        // @see MeMenuSoundSettingsSlider.as — position handle along the track
        _handle.x = _track.x + (_value * SLIDER_WIDTH) - (_handle.width / 2f);
    }

    private void OnMouseDown(WindowEvent ev, IWindow window)
    {
        _dragging = true;
    }

    private void OnMouseUp(WindowEvent ev, IWindow window)
    {
        _dragging = false;
    }

    private void OnMouseMove(WindowEvent ev, IWindow window)
    {
        if (!_dragging || _track == null)
        {
            return;
        }

        // TODO(as3-port): resolve mouse x relative to track — WindowEvent.mouseX adaptation needed
    }

    private void OnTrackClick(WindowEvent ev, IWindow window)
    {
        if (window != _track && window != _container)
        {
            return;
        }

        // TODO(as3-port): set value from click x position — WindowEvent.mouseX adaptation needed
    }

    private void SetValueFromX(float mouseX)
    {
        if (_track == null)
        {
            return;
        }

        float relative = (mouseX - _track.x) / SLIDER_WIDTH;
        SetValue(relative);
        ValueChanged?.Invoke(_value);
    }
}
