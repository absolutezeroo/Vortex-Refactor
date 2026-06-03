// @see habbo/window/widgets/FurnitureImageWidget.as

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Iterators;
using Vortex.Core.Window.Utils;
using Vortex.Habbo.Room;

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/FurnitureImageWidget.as
public class FurnitureImageWidget : IClass3618, IGetImageListener, IWidget
{
    public const string TYPE = "furniture_image";

    private const string FURNITURE_TYPE_KEY = "furniture_image:furnitureType";
    private const string SCALE_KEY = "furniture_image:scale";
    private const string DIRECTION_KEY = "furniture_image:direction";

    /// @see FurnitureImageWidget.as::const_417
    private static readonly string[] DIRECTIONS =
    {
        "northeast",
        "east",
        "southeast",
        "south",
        "southwest",
        "west",
        "northwest",
        "north",
    };

    private static readonly int[] SCALES =
    {
        32,
        64,
    };

    private static readonly PropertyStruct FURNITURE_TYPE_DEFAULT =
        new(FURNITURE_TYPE_KEY, "table_plasto_square", PropertyStruct.STRING);

    private static readonly PropertyStruct SCALE_DEFAULT =
        new(SCALE_KEY, 64, PropertyStruct.INT, false, System.Array.ConvertAll(SCALES, s => s.ToString()));

    private static readonly PropertyStruct DIRECTION_DEFAULT =
        new(DIRECTION_KEY, DIRECTIONS[2], PropertyStruct.STRING, false, DIRECTIONS);

    private const int ITEM_TYPE_FLOOR = 0;
    private const int ITEM_TYPE_WALL = 1;

    private IWidgetWindow? _widgetWindow;
    private HabboWindowManagerComponent? _windowManager;
    private IWindowContainer? _rootContainer;
    private IBitmapWrapperWindow? _bitmap;
    private IWindow? _regionWindow;
    private string _furnitureType = "table_plasto_square";
    private int _scale;
    private int _direction;
    private readonly Dictionary<int, string> _requestMap = new();
    private string? _extraData;
    private int _itemType;
    private IStuffData? _stuffData;

    /// @see FurnitureImageWidget.as::FurnitureImageWidget
    public FurnitureImageWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _scale = (int)SCALE_DEFAULT.value!;
        _direction = System.Array.IndexOf(DIRECTIONS, (string)DIRECTION_DEFAULT.value!);

        _widgetWindow = widgetWindow;
        _windowManager = windowManager;

        object? xmlAsset = windowManager.FindAssetByName("furniture_image_xml");
        if (xmlAsset is IAsset { Content: System.Xml.Linq.XElement xml })
        {
            _rootContainer = ((IWindowFactory)windowManager).BuildFromXml(xml) as IWindowContainer;
        }

        if (_rootContainer != null)
        {
            _bitmap = (_rootContainer as IWindow)?.GetChildByName("bitmap") as IBitmapWrapperWindow;
            IRegionWindow? region = (_rootContainer as IWindow)?.GetChildByName("region") as IRegionWindow;
            _regionWindow = region as IWindow;
            _regionWindow?.AddEventListener(WindowMouseEvent.CLICK, OnClick);
        }

        Refresh();

        if (_rootContainer != null)
        {
            _widgetWindow.RootWindow(_rootContainer);
            (_rootContainer as IWindow)!.width = _widgetWindow.width;
            (_rootContainer as IWindow)!.height = _widgetWindow.height;
        }
    }

    public bool disposed { get; private set; }

    /// @see FurnitureImageWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see FurnitureImageWidget.as::furnitureType
    string? IClass3618.FurnitureType
    {
        get => _furnitureType;
        set
        {
            if (value != null)
            {
                _furnitureType = value;
                Refresh();
            }
        }
    }

    /// @see FurnitureImageWidget.as::scale
    int IClass3618.Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            Refresh();
        }
    }

    /// @see FurnitureImageWidget.as::direction
    int IClass3618.Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            Refresh();
        }
    }

    /// @see FurnitureImageWidget.as::get properties
    public PropertyStruct[] Properties
    {
        get
        {
            if (disposed)
            {
                return System.Array.Empty<PropertyStruct>();
            }

            return new[]
            {
                FURNITURE_TYPE_DEFAULT.WithValue(_furnitureType),
                SCALE_DEFAULT.WithValue(_scale),
                DIRECTION_DEFAULT.WithValue(DIRECTIONS[_direction]),
            };
        }
        set
        {
            foreach (PropertyStruct prop in value)
            {
                switch (prop.key)
                {
                    case FURNITURE_TYPE_KEY:
                        _furnitureType = prop.value as string ?? "table_plasto_square";
                        break;
                    case SCALE_KEY:
                        if (prop.value is int i)
                        {
                            _scale = i;
                        }
                        break;
                    case DIRECTION_KEY:
                        _direction = System.Array.IndexOf(DIRECTIONS, prop.value as string);
                        break;
                }
            }
        }
    }

    /// @see FurnitureImageWidget.as::imageReady
    public void ImageReady(int param1, Image? param2)
    {
        if (_requestMap.TryGetValue(param1, out string? furnitureType) && furnitureType == _furnitureType)
        {
            Refresh();
        }
    }

    /// @see FurnitureImageWidget.as::imageFailed
    public void ImageFailed(int param1)
    {
    }

    /// @see FurnitureImageWidget.as::refresh
    private void Refresh()
    {
        if (disposed || _bitmap == null || _windowManager == null)
        {
            return;
        }

        _bitmap.Bitmap = null;

        // TODO(room-engine): Implement furniture image rendering when room engine is ported.
        // @see FurnitureImageWidget.as::refresh — AS3 calls:
        //   roomEngine.getFurnitureTypeId(furnitureType)
        //   roomEngine.getFurnitureImage(typeId, Vector3d(direction*45, 0, 0), scale, this, ...)  for floor items
        //   roomEngine.getWallItemImage(typeId, Vector3d(direction*45, 0, 0), scale, this, ...)   for wall items
        // The result (class_3499) has .id and .data; track request in _requestMap for async callback.

        // @see FurnitureImageWidget.as — placeholder fallback when bitmap is null or too small
        Image? currentBitmap = _bitmap.Bitmap;
        if (currentBitmap == null || currentBitmap.GetWidth() < 2)
        {
            string placeholderName = "placeholder_furni" + (_scale == 32 ? "_small" : "") + "_png";
            BitmapDataAsset? placeholderAsset = _windowManager.FindAssetByName(placeholderName) as BitmapDataAsset;
            if (placeholderAsset?.Content is Image placeholderImage)
            {
                _bitmap.Bitmap = placeholderImage;
                _bitmap.DisposesBitmap = false;
            }
        }

        (_bitmap as IWindow)?.Invalidate();

        currentBitmap = _bitmap.Bitmap;
        if (currentBitmap != null && _widgetWindow != null)
        {
            _widgetWindow.width = currentBitmap.GetWidth();
            _widgetWindow.height = currentBitmap.GetHeight();
        }
    }

    /// @see FurnitureImageWidget.as::onClick — empty in AS3
    private void OnClick(WindowEvent param1, IWindow param2)
    {
    }

    /// @see FurnitureImageWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (_regionWindow != null)
        {
            _regionWindow.RemoveEventListener(WindowMouseEvent.CLICK, OnClick);
            _regionWindow.Destroy();
            _regionWindow = null;
        }

        _bitmap = null;

        if (_rootContainer != null)
        {
            (_rootContainer as IWindow)?.Destroy();
            _rootContainer = null;
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
