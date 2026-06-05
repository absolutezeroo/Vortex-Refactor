// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/habbo/friendbar/landingview/layout/WidgetContainerLayout.as

using System;

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;

namespace Vortex.Habbo.FriendBar.LandingView;

/// @see com.sulake.habbo.friendbar.landingview.layout.WidgetContainerLayout
public sealed class WidgetContainerLayout : IDisposable
{
    private const string DEFAULT_LAYOUT = "landing_view_default_dynamic_layout";
    private const string GENERIC_RECEPTION_LAYOUT = "landing_view_generic_reception";

    private readonly HabboLandingView _landingView;
    private IWindowContainer? _window;
    private string? _layoutName;
    private int _orgWindowWidth;
    private int _orgWindowHeight;
    private bool _dynamicLayoutActive;
    private IDesktopWindow? _desktopWindow;

    /// @see WidgetContainerLayout.as::WidgetContainerLayout
    public WidgetContainerLayout(HabboLandingView landingView)
    {
        _landingView = landingView;
    }

    /// @see WidgetContainerLayout.as::window
    public IWindowContainer? Window => _window;

    /// @see WidgetContainerLayout.as::activate
    public void Activate()
    {
        if (_window == null)
        {
            CreateWindow();
        }

        if (_window == null)
        {
            return;
        }

        AttachToDesktop();
        ResizeWindow();

        _window.visible = true;
        _window.Invalidate();
    }

    /// @see WidgetContainerLayout.as::disable
    public void Disable()
    {
        if (_window != null)
        {
            _window.visible = false;
        }
    }

    /// @see WidgetContainerLayout.as::dispose
    public void Dispose()
    {
        if (_desktopWindow != null)
        {
            _desktopWindow.RemoveEventListener(WindowEvent.WE_RESIZED, OnDesktopResized);
            _desktopWindow = null;
        }

        _window?.Destroy();
        _window = null;
    }

    /// @see WidgetContainerLayout.as::createWindow
    private void CreateWindow()
    {
        if (_window != null)
        {
            return;
        }

        _layoutName = GetLayout();
        _window = _landingView.GetXmlWindow(_layoutName, HabboLandingView.LANDING_VIEW_LAYER) as IWindowContainer;

        if (_window == null && !string.Equals(_layoutName, GENERIC_RECEPTION_LAYOUT, StringComparison.Ordinal))
        {
            _layoutName = GENERIC_RECEPTION_LAYOUT;
            _window = _landingView.GetXmlWindow(GENERIC_RECEPTION_LAYOUT, HabboLandingView.LANDING_VIEW_LAYER) as IWindowContainer;
        }

        if (_window == null)
        {
            return;
        }

        HideWarningIfPresent();
        SetOrgWindowSize();
        SetupBottomSlotWidgetName();
        _dynamicLayoutActive = _window.FindChildByName("placeholder_dynamic_widget_slots") != null;
    }

    /// @see WidgetContainerLayout.as::hideWarningIfPresent
    private void HideWarningIfPresent()
    {
        IWindow? warning = _window?.FindChildByName("warning");

        if (warning != null)
        {
            warning.visible = false;
        }
    }

    /// @see WidgetContainerLayout.as::setOrgWindowSize
    private void SetOrgWindowSize()
    {
        if (_window == null)
        {
            return;
        }

        _orgWindowWidth = (int)_window.width;
        _orgWindowHeight = (int)_window.height;
    }

    /// @see WidgetContainerLayout.as::setupBottomSlotWidgetName
    private void SetupBottomSlotWidgetName()
    {
        IWindow? placeholder = _window?.FindChildByName("widget_placeholder_bottom_slot");

        if (placeholder == null)
        {
            return;
        }

        string widgetName = _landingView.GetProperty("landing.view.dynamic.slot.6.widget");

        if (string.IsNullOrEmpty(widgetName))
        {
            placeholder.visible = false;
        }
        else
        {
            placeholder.name = "widget_placeholder_" + widgetName;
        }
    }

    /// @see WidgetContainerLayout.as::getLayout
    private string GetLayout()
    {
        if (_landingView.PropertyExists("landing.view.layoutxml"))
        {
            string configuredLayout = _landingView.GetProperty("landing.view.layoutxml");

            if (!string.IsNullOrWhiteSpace(configuredLayout))
            {
                return configuredLayout;
            }
        }

        return DEFAULT_LAYOUT;
    }

    /// @see WidgetContainerLayout.as::resizeWindow
    private void ResizeWindow()
    {
        if (_window == null)
        {
            return;
        }

        _window.x = 0;
        _window.y = 0;

        IDesktopWindow? desktop = _landingView.WindowManager.GetWindowContext(HabboLandingView.LANDING_VIEW_LAYER)?.GetDesktopWindow();

        if (desktop == null)
        {
            return;
        }

        if (_dynamicLayoutActive)
        {
            ResizeDynamicLayout(desktop);
        }
        else
        {
            ResizeCustomLayout(desktop);
        }

        _window.Invalidate();
    }

    /// @see WidgetContainerLayout.as::resizeDynamicLayout
    private void ResizeDynamicLayout(IDesktopWindow desktop)
    {
        if (_window == null)
        {
            return;
        }

        _window.x = 0;
        _window.y = 0;
        _window.width = desktop.rectangle.Size.X;
        _window.height = desktop.rectangle.Size.Y;
    }

    /// @see WidgetContainerLayout.as::resizeCustomLayout
    private void ResizeCustomLayout(IDesktopWindow desktop)
    {
        if (_window == null)
        {
            return;
        }

        _window.width = _orgWindowWidth;
        _window.height = _orgWindowHeight;
        _window.x = Math.Max(0, (desktop.rectangle.Size.X - _window.width) / 2);

        if (desktop.rectangle.Size.Y > _window.height || IsGenericReceptionLayout())
        {
            _window.y = Math.Max(0, (desktop.rectangle.Size.Y - _window.height) / 2);
        }
        else
        {
            _window.y = desktop.rectangle.Size.Y - _window.height;
        }
    }

    /// @see WidgetContainerLayout.as::isGenericReceptionLayout
    private bool IsGenericReceptionLayout()
    {
        return string.Equals(_layoutName ?? GetLayout(), GENERIC_RECEPTION_LAYOUT, StringComparison.Ordinal);
    }

    private void AttachToDesktop()
    {
        if (_window == null)
        {
            return;
        }

        IDesktopWindow? desktop = _landingView.WindowManager.GetWindowContext(HabboLandingView.LANDING_VIEW_LAYER)?.GetDesktopWindow();

        if (desktop == null)
        {
            return;
        }

        if (_window.parent != null)
        {
            RegisterDesktopResize(desktop);
            return;
        }

        desktop.AddChild(_window);
        RegisterDesktopResize(desktop);
    }

    private void RegisterDesktopResize(IDesktopWindow desktop)
    {
        if (_desktopWindow == desktop)
        {
            return;
        }

        _desktopWindow?.RemoveEventListener(WindowEvent.WE_RESIZED, OnDesktopResized);
        _desktopWindow = desktop;
        _desktopWindow.AddEventListener(WindowEvent.WE_RESIZED, OnDesktopResized);
    }

    private void OnDesktopResized(WindowEvent ev, IWindow window)
    {
        _ = ev;
        _ = window;

        ResizeWindow();
    }
}
