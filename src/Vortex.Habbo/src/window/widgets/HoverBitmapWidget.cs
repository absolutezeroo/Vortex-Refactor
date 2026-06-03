// @see habbo/window/widgets/HoverBitmapWidget.as

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Iterators;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/HoverBitmapWidget.as
public class HoverBitmapWidget : IClass3563, IWidget
{
    private IWidgetWindow? _widgetWindow;
    private HabboWindowManagerComponent? _windowManager;
    private IStaticBitmapWrapperWindow? _bitmap;
    private IWindow? _bitmapWindow;
    private string? _normalAsset;
    private string? _hoverAsset;
    private bool _isHovering;

    /// @see habbo/window/widgets/HoverBitmapWidget.as::HoverBitmapWidget
    public HoverBitmapWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;

        // @see HoverBitmapWidget.as — build bitmap from hover_bitmap_xml asset
        object? xmlAsset = windowManager.FindAssetByName("hover_bitmap_xml");

        if (xmlAsset is Core.Assets.IAsset { Content: System.Xml.Linq.XElement xml })
        {
            IWindow? built = ((IWindowFactory)windowManager).BuildFromXml(xml);
            _bitmap = built as IStaticBitmapWrapperWindow;
            _bitmapWindow = built as IWindow;
        }

        if (_bitmapWindow == null)
        {
            return;
        }

        _bitmapWindow.AddEventListener("WME_OVER", OnMouseOver);
        _bitmapWindow.AddEventListener("WME_OUT", OnMouseOut);
        _widgetWindow.RootWindow(_bitmapWindow);

        _bitmapWindow.width = _widgetWindow.width;
        _bitmapWindow.height = _widgetWindow.height;

        _bitmapWindow.Invalidate();
    }

    /// @see habbo/window/widgets/HoverBitmapWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/HoverBitmapWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/HoverBitmapWidget.as::bitmapWrapper
    IStaticBitmapWrapperWindow? IClass3563.BitmapWrapper => _bitmap;

    /// @see habbo/window/widgets/HoverBitmapWidget.as::normalAsset
    string? IClass3563.NormalAsset
    {
        get => _normalAsset;
        set
        {
            _normalAsset = value;
            if (!_isHovering)
            {
                if (_bitmap != null) { _bitmap.AssetUri = _normalAsset; }
            }
        }
    }

    /// @see habbo/window/widgets/HoverBitmapWidget.as::hoverAsset
    string? IClass3563.HoverAsset
    {
        get => _hoverAsset;
        set
        {
            _hoverAsset = value;
            if (_isHovering)
            {
                if (_bitmap != null) { _bitmap.AssetUri = _hoverAsset; }
            }
        }
    }

    /// @see habbo/window/widgets/HoverBitmapWidget.as::onMouseOver
    private void OnMouseOver(WindowEvent param1, IWindow param2)
    {
        _isHovering = true;

        if (_bitmap != null) { _bitmap.AssetUri = _hoverAsset; }
    }

    /// @see habbo/window/widgets/HoverBitmapWidget.as::onMouseOut
    private void OnMouseOut(WindowEvent param1, IWindow param2)
    {
        _isHovering = false;

        if (_bitmap != null) { _bitmap.AssetUri = _normalAsset; }
    }

    /// @see habbo/window/widgets/HoverBitmapWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (_bitmapWindow != null)
        {
            _bitmapWindow.Destroy();
            _bitmapWindow = null;
            _bitmap = null;
        }

        if (_widgetWindow != null)
        {
            _widgetWindow.RootWindow(null);
            _widgetWindow = null;
        }

        _windowManager = null;
        disposed = true;
    }
}
