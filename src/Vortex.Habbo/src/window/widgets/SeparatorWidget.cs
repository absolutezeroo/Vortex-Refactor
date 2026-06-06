// @see habbo/window/widgets/SeparatorWidget.as

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Iterators;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/SeparatorWidget.as
public class SeparatorWidget : ISeparatorWidget, IWidget
{
    private const string BORDER_IMAGE_HORIZONTAL = "illumina_light_separator_horizontal";
    private const string BORDER_IMAGE_VERTICAL = "illumina_light_separator_vertical";

    private readonly IWidgetWindow _widgetWindow;
    private readonly HabboWindowManagerComponent _windowManager;
    private IWindowContainer? _rootContainer;
    private IBitmapWrapperWindow? _canvas;
    private IWindowContainer? _children;
    private bool _vertical;

    /// @see habbo/window/widgets/SeparatorWidget.as::SeparatorWidget
    public SeparatorWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;

        // @see SeparatorWidget.as — build root from separator_xml asset
        object? xmlAsset = windowManager.FindAssetByName("separator_xml");
        if (xmlAsset is IAsset { Content: System.Xml.Linq.XElement xml })
        {
            _rootContainer = ((IWindowFactory)windowManager).BuildFromXml(xml) as IWindowContainer;
        }

        if (_rootContainer != null)
        {
            _canvas = _rootContainer.GetChildByName("canvas") as IBitmapWrapperWindow;
            _children = _rootContainer.GetChildByName("children") as IWindowContainer;
            _widgetWindow.RootWindow(_rootContainer);
            _rootContainer.width = _widgetWindow.width;
            _rootContainer.height = _widgetWindow.height;
        }

        Refresh();
    }

    /// @see habbo/window/widgets/SeparatorWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/SeparatorWidget.as::get iterator
    public object? Iterator()
    {
        return (_children as IWindow)?.Iterator() ?? EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/SeparatorWidget.as::vertical
    bool ISeparatorWidget.Vertical
    {
        get => _vertical;
        set
        {
            _vertical = value;
            Refresh();
        }
    }

    /// @see habbo/window/widgets/SeparatorWidget.as::onChange
    public void OnChange()
    {
        Refresh();
    }

    /// @see habbo/window/widgets/SeparatorWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        _canvas = null;
        _children = null;

        if (_rootContainer != null)
        {
            _rootContainer.Destroy();
            _rootContainer = null;
        }

        _widgetWindow.RootWindow(null);
        disposed = true;
    }

    /// @see habbo/window/widgets/SeparatorWidget.as::refresh
    private void Refresh()
    {
        if (disposed || _canvas == null)
        {
            return;
        }

        // @see SeparatorWidget.as — load separator asset and tile across canvas
        string assetName = _vertical ? BORDER_IMAGE_VERTICAL : BORDER_IMAGE_HORIZONTAL;
        BitmapDataAsset? asset = _windowManager.FindAssetByName(assetName) as BitmapDataAsset;
        if (asset?.Content is not Image sourceImage)
        {
            return;
        }

        IWindow? canvasWin = _canvas as IWindow;
        int canvasW = (int)System.Math.Max(1, canvasWin?.width ?? 1);
        int canvasH = (int)System.Math.Max(1, canvasWin?.height ?? 1);
        int srcW = sourceImage.GetWidth();
        int srcH = sourceImage.GetHeight();

        if (srcW <= 0 || srcH <= 0)
        {
            return;
        }

        // @see SeparatorWidget.as — tile the source image across the canvas area
        Image? result = Image.CreateEmpty(canvasW, canvasH, false, Image.Format.Rgba8);

        if (_vertical)
        {
            int px = (canvasW / 2) - 1;
            for (int py = 0;
                 py < canvasH;
                 py += srcH)
            {
                result.BlitRect(sourceImage, new Rect2I(0, 0, srcW, srcH), new Vector2I(px, py));
            }
        }
        else
        {
            int py = (canvasH / 2) - 1;
            for (int px = 0;
                 px < canvasW;
                 px += srcW)
            {
                result.BlitRect(sourceImage, new Rect2I(0, 0, srcW, srcH), new Vector2I(px, py));
            }
        }

        _canvas.Bitmap = result;
    }
}
