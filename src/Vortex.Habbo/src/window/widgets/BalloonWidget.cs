// @see habbo/window/widgets/BalloonWidget.as

using System;

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Iterators;
using Vortex.Habbo.Window.Enum;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/BalloonWidget.as
public class BalloonWidget : IClass3583, IWidget
{
    public const string ARROW_ASSET_PREFIX = "illumina_light_balloon_arrow_";
    public const int ARROW_FREE_PADDING = 6;
    public const int ARROW_LENGTH = 6;
    public const int ARROW_WIDTH = 9;

    private IWidgetWindow? _widgetWindow;
    private HabboWindowManagerComponent? _windowManager;
    private readonly bool _batchingProperties;
    private bool _resizing;
    private IWindowContainer? _container;
    private IWindowContainer? _border;
    private IStaticBitmapWrapperWindow? _arrowBitmap;
    private IWindow? _arrowBitmapWindow;
    private string _arrowPivot = "up, center";
    private int _arrowDisplacement;

    /// @see habbo/window/widgets/BalloonWidget.as::BalloonWidget
    public BalloonWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;

        // @see BalloonWidget.as — build root from balloon_xml asset
        object? xmlAsset = windowManager.FindAssetByName("balloon_xml");
        if (xmlAsset is Core.Assets.IAsset { Content: System.Xml.Linq.XElement xml })
        {
            _container = ((IWindowFactory)windowManager).BuildFromXml(xml) as IWindowContainer;
        }

        if (_container == null)
        {
            return;
        }

        IWindow? containerWin = _container;
        _arrowBitmapWindow = containerWin.FindChildByName("bitmap");
        _arrowBitmap = _arrowBitmapWindow as IStaticBitmapWrapperWindow;
        _border = containerWin.FindChildByName("border") as IWindowContainer;

        SyncFlags();

        _widgetWindow.AddEventListener("WE_RESIZE", OnChange);
        _widgetWindow.AddEventListener("WE_RESIZED", OnChange);
        if (_border is IWindow borderWin)
        {
            borderWin.AddEventListener("WE_RESIZE", OnChange);
            borderWin.AddEventListener("WE_RESIZED", OnChange);
        }

        _widgetWindow.RootWindow(_container);
        containerWin.width = _widgetWindow.width;
        containerWin.height = _widgetWindow.height;
    }

    /// @see habbo/window/widgets/BalloonWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/BalloonWidget.as::get iterator
    public object? Iterator()
    {
        return _border is IWindow w ? w.Iterator() : EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/BalloonWidget.as::arrowPivot
    string IClass3583.ArrowPivot
    {
        get => _arrowPivot;
        set
        {
            _arrowPivot = value;
            ClearFlags();
            Refresh();
            SyncFlags();
            Refresh();
        }
    }

    /// @see habbo/window/widgets/BalloonWidget.as::arrowDisplacement
    int IClass3583.ArrowDisplacement
    {
        get => _arrowDisplacement;
        set
        {
            _arrowDisplacement = value;
            Refresh();
        }
    }

    /// @see habbo/window/widgets/BalloonWidget.as::onChange
    private void OnChange(WindowEvent param1, IWindow param2)
    {
        Refresh();
    }

    /// @see habbo/window/widgets/BalloonWidget.as::syncFlags
    public void SyncFlags()
    {
        if (_border is not IWindow borderWin || _widgetWindow == null)
        {
            return;
        }

        borderWin.SetParamFlag(131072, _widgetWindow.GetParamFlag(131072));
        borderWin.SetParamFlag(147456, _widgetWindow.GetParamFlag(147456));
    }

    /// @see habbo/window/widgets/BalloonWidget.as::clearFlags
    public void ClearFlags()
    {
        if (_border is not IWindow borderWin)
        {
            return;
        }

        borderWin.SetParamFlag(131072, false);
        borderWin.SetParamFlag(147456, false);
    }

    /// @see habbo/window/widgets/BalloonWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (_border is IWindow borderWin)
        {
            borderWin.RemoveEventListener("WE_RESIZE", OnChange);
            borderWin.RemoveEventListener("WE_RESIZED", OnChange);
            _border = null;
        }

        _arrowBitmap = null;
        _arrowBitmapWindow = null;

        if (_container is IWindow containerWin)
        {
            containerWin.Destroy();
            _container = null;
        }

        if (_widgetWindow != null)
        {
            _widgetWindow.RemoveEventListener("WE_RESIZE", OnChange);
            _widgetWindow.RemoveEventListener("WE_RESIZED", OnChange);
            _widgetWindow.RootWindow(null);
            _widgetWindow = null;
        }

        _windowManager = null;
        disposed = true;
    }

    /// @see habbo/window/widgets/BalloonWidget.as::refresh
    private void Refresh()
    {
        if (_batchingProperties || _resizing || disposed || _border == null || _container == null || _widgetWindow == null)
        {
            return;
        }

        IWindow? containerWin = _container;
        IWindow? borderWin = _border;

        string direction = Class3821.DirectionFromPivot(_arrowPivot);

        int w,
            h;

        switch (direction)
        {
            case "up":
            case "down":
                w = (int)borderWin.width;
                h = (int)borderWin.height + ARROW_LENGTH - 1;
                break;
            case "left":
            case "right":
                w = (int)borderWin.width + ARROW_LENGTH - 1;
                h = (int)borderWin.height;
                break;
            default:
                return;
        }

        _resizing = true;

        if (_widgetWindow.TestParamFlag(147456))
        {
            containerWin.width = w;
            containerWin.height = h;
        }
        else if (_widgetWindow.TestParamFlag(131072))
        {
            containerWin.width = Math.Max(_widgetWindow.width, w);
            containerWin.height = Math.Max(_widgetWindow.height, h);
        }
        else
        {
            containerWin.width = _widgetWindow.width;
            containerWin.height = _widgetWindow.height;
        }

        _widgetWindow.width = containerWin.width;
        _widgetWindow.height = containerWin.height;
        _resizing = false;

        if (_arrowBitmap != null) { _arrowBitmap.AssetUri = ARROW_ASSET_PREFIX + direction; }

        string position = Class3821.PositionFromPivot(_arrowPivot);
        int pos;

        switch (direction)
        {
            case "up":
            case "down":
                pos = position switch
                {
                    "minimum" => ARROW_FREE_PADDING,
                    "middle" => (int)((containerWin.width - ARROW_WIDTH) / 2),
                    "maximum" => (int)(containerWin.width - ARROW_FREE_PADDING - ARROW_WIDTH),
                    _ => (int)((containerWin.width - ARROW_WIDTH) / 2),
                };

                _resizing = true;
                borderWin.SetRectangle(
                    0,
                    direction == "up" ? ARROW_LENGTH - 1 : 0,
                    containerWin.width,
                    containerWin.height + 1 - ARROW_LENGTH
                );
                _resizing = false;

                _arrowBitmapWindow?.SetRectangle(
                    Math.Clamp(pos + _arrowDisplacement, ARROW_FREE_PADDING, (int)containerWin.width - ARROW_FREE_PADDING),
                    direction == "up" ? 0 : borderWin.bottom - 1,
                    ARROW_WIDTH,
                    ARROW_LENGTH
                );
                break;

            case "left":
            case "right":
                pos = position switch
                {
                    "minimum" => ARROW_FREE_PADDING,
                    "middle" => (int)((containerWin.height - ARROW_WIDTH) / 2),
                    "maximum" => (int)(containerWin.height - ARROW_FREE_PADDING - ARROW_WIDTH),
                    _ => (int)((containerWin.height - ARROW_WIDTH) / 2),
                };

                _resizing = true;

                borderWin.SetRectangle(
                    direction == "left" ? ARROW_LENGTH - 1 : 0,
                    0,
                    containerWin.width + 1 - ARROW_LENGTH,
                    containerWin.height
                );

                _resizing = false;

                _arrowBitmapWindow?.SetRectangle(
                    direction == "left" ? 0 : borderWin.right - 1,
                    Math.Clamp(pos + _arrowDisplacement, ARROW_FREE_PADDING, (int)containerWin.height - ARROW_FREE_PADDING),
                    ARROW_LENGTH,
                    ARROW_WIDTH
                );
                break;
        }
    }
}
