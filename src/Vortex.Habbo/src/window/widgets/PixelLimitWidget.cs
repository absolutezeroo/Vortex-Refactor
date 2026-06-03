// @see habbo/window/widgets/PixelLimitWidget.as

using System;

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Iterators;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/PixelLimitWidget.as
public class PixelLimitWidget : IClass3530, IWidget
{
    private IWidgetWindow? _widgetWindow;
    private HabboWindowManagerComponent? _windowManager;
    private readonly bool _batchingProperties;
    private IWindowContainer? _container;
    private IStaticBitmapWrapperWindow? _bitmap;
    private int _limit;

    /// @see habbo/window/widgets/PixelLimitWidget.as::PixelLimitWidget
    public PixelLimitWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;

        // @see PixelLimitWidget.as — build root from badge_image_xml asset
        object? xmlAsset = windowManager.FindAssetByName("badge_image_xml");

        if (xmlAsset is Core.Assets.IAsset { Content: System.Xml.Linq.XElement xml })
        {
            _container = ((IWindowFactory)windowManager).BuildFromXml(xml) as IWindowContainer;
        }

        if (_container == null)
        {
            return;
        }

        _bitmap = _container.FindChildByName("bitmap") as IStaticBitmapWrapperWindow;
        _widgetWindow.RootWindow(_container);
        _container.width = _widgetWindow.width;
        _container.height = _widgetWindow.height;
    }

    /// @see habbo/window/widgets/PixelLimitWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/PixelLimitWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/PixelLimitWidget.as::limit
    int IClass3530.Limit
    {
        get => _limit;
        set
        {
            _limit = Math.Clamp(value, 0, 100);
            Refresh();
        }
    }

    /// @see habbo/window/widgets/PixelLimitWidget.as::get assetUri
    public string AssetUri
    {
        get
        {
            // @see PixelLimitWidget.as — round limit to nearest 20, min 20
            int rounded = (int)(Math.Floor(_limit / 20.0) * 20);
            rounded = Math.Max(rounded, 20);

            return "${image.library.url}reception/challenge_meter_" + rounded + ".png";
        }
    }

    /// @see habbo/window/widgets/PixelLimitWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        _bitmap = null;

        if (_container is IWindow containerWin)
        {
            containerWin.Destroy();
            _container = null;
        }

        if (_widgetWindow != null)
        {
            _widgetWindow.RootWindow(null);
            _widgetWindow = null;
        }

        _windowManager = null;
        disposed = true;
    }

    /// @see habbo/window/widgets/PixelLimitWidget.as::refresh
    private void Refresh()
    {
        if (_batchingProperties)
        {
            return;
        }

        if (_bitmap != null) { _bitmap.AssetUri = AssetUri; }

        if (_bitmap is IWindow bitmapWin)
        {
            bitmapWin.Invalidate();
        }
    }
}
