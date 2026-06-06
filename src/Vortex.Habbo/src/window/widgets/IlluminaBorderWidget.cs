// @see habbo/window/widgets/IlluminaBorderWidget.as

using System;

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Iterators;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/IlluminaBorderWidget.as
public class IlluminaBorderWidget : IIlluminaBorderWidget, IWidget
{
    public const string BORDER_STYLE_ILLUMINA_LIGHT = "illumina_light";
    public const string BORDER_STYLE_ILLUMINA_DARK = "illumina_dark";
    public static readonly string[] BORDER_STYLES =
    {
        BORDER_STYLE_ILLUMINA_LIGHT,
        BORDER_STYLE_ILLUMINA_DARK,
    };
    public const string BORDER_STYLE_KEY = "illumina_border:border_style";

    private static readonly string[] BORDER_PIECES =
    {
        "top_left",
        "top_center",
        "top_right",
        "center_right",
        "bottom_right",
        "bottom_center",
        "bottom_left",
        "center_left",
    };

    private IWidgetWindow? _widgetWindow;
    private HabboWindowManagerComponent? _windowManager;
    private IWindowContainer? _container;
    private IBitmapWrapperWindow? _canvas;
    private IWindow? _canvasWindow;
    private IWindowContainer? _children;
    private readonly bool _batchingProperties;
    private bool _resizing;
    private string _borderStyle = "illumina_light";
    private string _contentChild = "";
    private uint _contentPadding = 5;
    private uint _sidePadding = 15;
    private uint _childMargin = 3;
    private string _topLeftChild = "";
    private string _topCenterChild = "";
    private string _topRightChild = "";
    private string _bottomLeftChild = "";
    private string _bottomCenterChild = "";
    private string _bottomRightChild = "";
    private bool _landingViewMode;

    // @see IlluminaBorderWidget.as — cached border piece assets keyed by piece name
    private readonly Dictionary<string, BitmapDataAsset?> _borderAssets = new();

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::IlluminaBorderWidget
    public IlluminaBorderWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;

        // @see IlluminaBorderWidget.as — build root from illumina_border_xml asset
        object? xmlAsset = windowManager.FindAssetByName("illumina_border_xml");
        if (xmlAsset is IAsset { Content: System.Xml.Linq.XElement xml })
        {
            _container = ((IWindowFactory)windowManager).BuildFromXml(xml) as IWindowContainer;
        }

        if (_container == null)
        {
            return;
        }

        IWindow? containerWin = _container;
        _canvasWindow = containerWin.GetChildByName("canvas");
        _canvas = _canvasWindow as IBitmapWrapperWindow;
        _children = containerWin.GetChildByName("children") as IWindowContainer;

        // @see IlluminaBorderWidget.as — load default border style
        LoadBorderAssets(_borderStyle);

        _widgetWindow.AddEventListener("WE_RESIZE", OnChange);
        _widgetWindow.AddEventListener("WE_RESIZED", OnChange);

        if (_children is IWindow childrenWin)
        {
            childrenWin.AddEventListener("WE_CHILD_ADDED", OnChange);
            childrenWin.AddEventListener("WE_CHILD_REMOVED", OnChange);
            childrenWin.AddEventListener("WE_CHILD_RELOCATED", OnChange);
            childrenWin.AddEventListener("WE_CHILD_RESIZED", OnChange);
        }

        _widgetWindow.RootWindow(_container);

        containerWin.width = _widgetWindow.width;
        containerWin.height = _widgetWindow.height;
    }

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::get iterator
    public object? Iterator()
    {
        return _children is IWindow w ? w.Iterator() : EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::borderStyle
    string IIlluminaBorderWidget.BorderStyle
    {
        get => _borderStyle;
        set { _borderStyle = value; LoadBorderAssets(value); Refresh(); }
    }

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::contentChild
    string IIlluminaBorderWidget.ContentChild
    {
        get => _contentChild;
        set { _contentChild = value ?? ""; Refresh(); }
    }

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::contentPadding
    uint IIlluminaBorderWidget.ContentPadding
    {
        get => _contentPadding;
        set { _contentPadding = value; Refresh(); }
    }

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::sidePadding
    uint IIlluminaBorderWidget.SidePadding
    {
        get => _sidePadding;
        set { _sidePadding = value; Refresh(); }
    }

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::childMargin
    uint IIlluminaBorderWidget.ChildMargin
    {
        get => _childMargin;
        set { _childMargin = value; Refresh(); }
    }

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::topLeftChild
    string IIlluminaBorderWidget.TopLeftChild
    {
        get => _topLeftChild;
        set { _topLeftChild = value ?? ""; Refresh(); }
    }

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::topCenterChild
    string IIlluminaBorderWidget.TopCenterChild
    {
        get => _topCenterChild;
        set { _topCenterChild = value ?? ""; Refresh(); }
    }

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::topRightChild
    string IIlluminaBorderWidget.TopRightChild
    {
        get => _topRightChild;
        set { _topRightChild = value ?? ""; Refresh(); }
    }

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::bottomLeftChild
    string IIlluminaBorderWidget.BottomLeftChild
    {
        get => _bottomLeftChild;
        set { _bottomLeftChild = value ?? ""; Refresh(); }
    }

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::bottomCenterChild
    string IIlluminaBorderWidget.BottomCenterChild
    {
        get => _bottomCenterChild;
        set { _bottomCenterChild = value ?? ""; Refresh(); }
    }

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::bottomRightChild
    string IIlluminaBorderWidget.BottomRightChild
    {
        get => _bottomRightChild;
        set { _bottomRightChild = value ?? ""; Refresh(); }
    }

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::landingViewMode
    bool IIlluminaBorderWidget.LandingViewMode
    {
        get => _landingViewMode;
        set { _landingViewMode = value; Refresh(); }
    }

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::onChange
    public void OnChange()
    {
        Refresh();
    }

    private void OnChange(WindowEvent param1, IWindow param2)
    {
        Refresh();
    }

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (_canvasWindow != null)
        {
            _canvasWindow.RemoveEventListener("WE_RESIZE", OnChange);
            _canvasWindow.RemoveEventListener("WE_RESIZED", OnChange);
            _canvasWindow = null;
            _canvas = null;
        }

        if (_children is IWindow childrenWin)
        {
            childrenWin.RemoveEventListener("WE_CHILD_ADDED", OnChange);
            childrenWin.RemoveEventListener("WE_CHILD_REMOVED", OnChange);
            childrenWin.RemoveEventListener("WE_CHILD_RELOCATED", OnChange);
            childrenWin.RemoveEventListener("WE_CHILD_RESIZED", OnChange);
            _children = null;
        }

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

        _borderAssets.Clear();
        _windowManager = null;
        disposed = true;
    }

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::getPiece
    public static string GetPiece(string borderStyle, string pieceName)
    {
        return borderStyle + "_" + pieceName;
    }

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::getChildHeight
    public int GetChildHeight(string childName)
    {
        if (string.IsNullOrEmpty(childName) || _children == null)
        {
            return 0;
        }

        IWindow? child = ((IWindow)_children).GetChildByName(childName);

        return child != null ? (int)child.height : 0;
    }

    /// @see IlluminaBorderWidget.as — load all 8 border piece assets for a style
    private void LoadBorderAssets(string style)
    {
        _borderAssets.Clear();

        if (_windowManager == null)
        {
            return;
        }

        foreach (string piece in BORDER_PIECES)
        {
            _borderAssets[piece] = _windowManager.FindAssetByName(style + "_border_" + piece) as BitmapDataAsset;
        }
    }

    private int TopPadding =>
        Math.Max(
            GetChildHeight(_topCenterChild),
            Math.Max(GetChildHeight(_topLeftChild), GetChildHeight(_topRightChild))
        ) / 2;

    private int BottomPadding =>
        Math.Max(
            GetChildHeight(_bottomCenterChild),
            Math.Max(GetChildHeight(_bottomLeftChild), GetChildHeight(_bottomRightChild))
        ) / 2;

    /// @see habbo/window/widgets/IlluminaBorderWidget.as::refresh
    private void Refresh()
    {
        if (_batchingProperties || _resizing || disposed || _container == null || _canvas == null || _canvasWindow == null ||
            _children == null || _widgetWindow == null)
        {
            return;
        }

        IWindow? containerWin = _container;
        IWindow? childrenWin = _children;

        containerWin.width = _widgetWindow.width;
        containerWin.height = _widgetWindow.height;

        // @see IlluminaBorderWidget.as — resize based on content child + padding
        IWindow? contentWin = childrenWin.GetChildByName(_contentChild);

        if (contentWin != null)
        {
            int minW = (int)Math.Max(1, contentWin.width + (2 * _contentPadding));
            int minH = (int)Math.Max(1, contentWin.height + (2 * _contentPadding) + TopPadding + BottomPadding);

            _resizing = true;

            if (_widgetWindow.TestParamFlag(147456))
            {
                containerWin.width = minW;
                containerWin.height = minH;
            }
            else if (_widgetWindow.TestParamFlag(131072))
            {
                containerWin.width = Math.Max(containerWin.width, minW);
                containerWin.height = Math.Max(containerWin.height, minH);
            }

            _resizing = false;
        }

        // @see IlluminaBorderWidget.as — resize canvas and children containers
        _canvasWindow.width = containerWin.width;
        _canvasWindow.height = containerWin.height;
        childrenWin.width = containerWin.width;
        childrenWin.height = containerWin.height;

        // @see IlluminaBorderWidget.as — create bitmap for border rendering
        int cw = (int)Math.Max(1, _canvasWindow.width);
        int ch = (int)Math.Max(1, _canvasWindow.height);

        Image? bitmap = Image.CreateEmpty(cw, ch, false, Image.Format.Rgba8);

        // @see IlluminaBorderWidget.as — compute adjusted rectangle with top/bottom padding
        int rectX = 0;
        int rectY = TopPadding;
        int rectW = cw;
        int rectH = ch - TopPadding - BottomPadding;

        // @see IlluminaBorderWidget.as — draw each border piece scaled to its target area
        foreach (string pieceName in BORDER_PIECES)
        {
            if (!_borderAssets.TryGetValue(pieceName, out BitmapDataAsset? asset) || asset == null)
            {
                continue;
            }

            // @see IlluminaBorderWidget.as — skip left pieces in landing view mode
            if (_landingViewMode && pieceName is "top_left" or "center_left" or "bottom_left")
            {
                continue;
            }

            if (asset.Content is not Image srcImage)
            {
                continue;
            }

            int srcW = srcImage.GetWidth();
            int srcH = srcImage.GetHeight();

            if (srcW <= 0 || srcH <= 0)
            {
                continue;
            }

            int dx = rectX,
                dy = rectY,
                dw = srcW,
                dh = srcH;

            // @see IlluminaBorderWidget.as — compute destination rectangle per piece
            switch (pieceName)
            {
                case "top_left":
                    break;
                case "top_center":
                    dx += GetPieceWidth("top_left");
                    dw = rectW - GetPieceWidth("top_left") - GetPieceWidth("top_right");
                    break;
                case "top_right":
                    dx += rectW - srcW;
                    break;
                case "center_right":
                    dx += rectW - srcW;
                    dy += GetPieceHeight("top_right");
                    dh = rectH - GetPieceHeight("top_right") - GetPieceHeight("bottom_right");
                    break;
                case "bottom_right":
                    dx += rectW - srcW;
                    dy += rectH - srcH;
                    break;
                case "bottom_center":
                    dx += GetPieceWidth("bottom_left");
                    dy += rectH - srcH;
                    dw = rectW - GetPieceWidth("bottom_left") - GetPieceWidth("bottom_right");
                    if (_landingViewMode)
                    {
                        int half = dw / 2;
                        dx += half;
                        dw -= half;
                    }
                    break;
                case "bottom_left":
                    dy += rectH - srcH;
                    break;
                case "center_left":
                    dy += GetPieceHeight("top_left");
                    dh = rectH - GetPieceHeight("top_left") - GetPieceHeight("bottom_left");
                    break;
                default:
                    continue;
            }

            // @see IlluminaBorderWidget.as — draw scaled piece via matrix transform
            if (dw <= 0 || dh <= 0)
            {
                continue;
            }

            Image? scaled = Image.CreateEmpty(dw, dh, false, Image.Format.Rgba8);
            scaled.CopyFrom(srcImage);
            scaled.Resize(dw, dh, Image.Interpolation.Bilinear);
            bitmap.BlitRect(scaled, new Rect2I(0, 0, dw, dh), new Vector2I(dx, dy));
        }

        // @see IlluminaBorderWidget.as — position children (corner/edge children and content child)
        string[] cornerChildNames =
            [_topLeftChild, _topCenterChild, _topRightChild, _bottomLeftChild, _bottomCenterChild, _bottomRightChild];

        // iterate through children window's children
        for (int i = 0;
             i < childrenWin.numChildren;
             i++)
        {
            IWindow? child = childrenWin.GetChildAt(i);
            if (child == null || string.IsNullOrEmpty(child.name))
            {
                if (child != null)
                {
                    child.visible = false;
                }
                continue;
            }

            int cornerIndex = Array.IndexOf(cornerChildNames, child.name);
            if (cornerIndex < 0)
            {
                // @see IlluminaBorderWidget.as — content child or hidden
                if (child.name == _contentChild)
                {
                    child.x = rectX + _contentPadding;
                    child.y = rectY + _contentPadding;
                    child.visible = true;
                }
                else
                {
                    child.visible = false;
                }
            }
            else
            {
                // @see IlluminaBorderWidget.as — position corner/edge child by index
                child.x = (cornerIndex % 3) switch
                {
                    0 => Math.Min(_sidePadding, _canvasWindow.width - child.width),
                    1 => Math.Max(_canvasWindow.width - child.width, 0) / 2,
                    2 => Math.Max(_canvasWindow.width - child.width - _sidePadding, 0),
                    _ => child.x,
                };

                if (cornerIndex < 3)
                {
                    child.y = TopPadding - (child.height / 2);
                }
                else
                {
                    child.y = _canvasWindow.height - (BottomPadding + (child.height / 2));
                }

                child.visible = true;

                // @see IlluminaBorderWidget.as — clear bitmap area around child for transparency
                int clearX = (int)(child.x - _childMargin);
                int clearY = (int)child.y;
                int clearW = (int)(child.width + (_childMargin * 2));
                int clearH = (int)child.height;

                if (clearX < 0 || clearY < 0 || clearX + clearW > cw || clearY + clearH > ch)
                {
                    continue;
                }

                for (int py = clearY;
                     py < clearY + clearH && py < ch;
                     py++)
                {
                    for (int px = clearX;
                         px < clearX + clearW && px < cw;
                         px++)
                    {
                        bitmap.SetPixel(px, py, new Color(0, 0, 0, 0));
                    }
                }
            }
        }

        _canvas.Bitmap = bitmap;
        _canvasWindow.Invalidate();
    }

    private int GetPieceWidth(string pieceName)
    {
        if (_borderAssets.TryGetValue(pieceName, out BitmapDataAsset? asset) && asset?.Content is Image img)
        {
            return img.GetWidth();
        }

        return 0;
    }

    private int GetPieceHeight(string pieceName)
    {
        if (_borderAssets.TryGetValue(pieceName, out BitmapDataAsset? asset) && asset?.Content is Image img)
        {
            return img.GetHeight();
        }

        return 0;
    }
}
