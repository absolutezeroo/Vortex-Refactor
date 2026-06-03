// @see habbo/window/widgets/AvatarImageWidget.as

using System;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Iterators;
using Vortex.Core.Window.Utils;
using Vortex.Habbo.Avatar;

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/AvatarImageWidget.as
public class AvatarImageWidget : IAvatarImageWidget, IAvatarImageListener
{
    public const string TYPE = "avatar_image";

    private const string FIGURE_KEY = "avatar_image:figure";
    private const string SCALE_KEY = "avatar_image:scale";
    private const string ONLY_HEAD_KEY = "avatar_image:only_head";
    private const string CROPPED_KEY = "avatar_image:cropped";
    private const string DIRECTION_KEY = "avatar_image:direction";

    /// @see AvatarImageWidget.as::const_417
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

    private static readonly PropertyStruct FIGURE_DEFAULT =
        new(FIGURE_KEY, "hd-180-1.ch-210-66.lg-270-82.sh-290-81", PropertyStruct.STRING);

    private static readonly PropertyStruct SCALE_DEFAULT =
        new(SCALE_KEY, "h", PropertyStruct.STRING, false, new[]
        {
            "sh",
            "h",
        });

    private static readonly PropertyStruct ONLY_HEAD_DEFAULT =
        new(ONLY_HEAD_KEY, false, PropertyStruct.BOOLEAN);

    private static readonly PropertyStruct CROPPED_DEFAULT =
        new(CROPPED_KEY, false, PropertyStruct.BOOLEAN);

    private static readonly PropertyStruct DIRECTION_DEFAULT =
        new(DIRECTION_KEY, DIRECTIONS[2], PropertyStruct.STRING, false, DIRECTIONS);

    private const double RC = 0.3333333333333333;
    private const double GC = 0.3333333333333333;
    private const double BC = 0.3333333333333333;

    private IWidgetWindow? _widgetWindow;
    private HabboWindowManagerComponent? _windowManager;
    private IWindowContainer? _rootContainer;
    private IBitmapWrapperWindow? _bitmap;
    private IWindow? _regionWindow;
    private string _figure;
    private bool _emptyFigure;
    private string _scale;
    private bool _onlyHead;
    private bool _cropped;
    private int _direction;
    private int _userId;

    /// @see AvatarImageWidget.as::AvatarImageWidget
    public AvatarImageWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _figure = (string)FIGURE_DEFAULT.value!;
        _scale = (string)SCALE_DEFAULT.value!;
        _onlyHead = (bool)ONLY_HEAD_DEFAULT.value!;
        _cropped = (bool)CROPPED_DEFAULT.value!;
        _direction = Array.IndexOf(DIRECTIONS, (string)DIRECTION_DEFAULT.value!);

        _widgetWindow = widgetWindow;
        _windowManager = windowManager;

        object? xmlAsset = windowManager.FindAssetByName("avatar_image_xml");

        if (xmlAsset is IAsset { Content: XElement xml })
        {
            _rootContainer = ((IWindowFactory)windowManager).BuildFromXml(xml) as IWindowContainer;
        }

        if (_rootContainer != null)
        {
            _bitmap = _rootContainer?.GetChildByName("bitmap") as IBitmapWrapperWindow;
            IRegionWindow? region = _rootContainer?.GetChildByName("region") as IRegionWindow;
            _regionWindow = region as IWindow;
            _regionWindow?.AddEventListener(WindowMouseEvent.CLICK, OnClick);
        }

        Refresh();

        if (_rootContainer == null)
        {
            return;
        }

        _widgetWindow.RootWindow(_rootContainer);
        _rootContainer!.width = _widgetWindow.width;
        _rootContainer!.height = _widgetWindow.height;
    }

    /// @see AvatarImageWidget.as::cleanupAvatarString
    private static string CleanupAvatarString(string? param1)
    {
        if (string.IsNullOrEmpty(param1))
        {
            return (string)FIGURE_DEFAULT.value!;
        }

        return System.Text.RegularExpressions.Regex.Replace(param1, "NaN", "");
    }

    public bool disposed { get; private set; }

    /// @see AvatarImageWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see AvatarImageWidget.as::get figure
    public string? Figure
    {
        get => _figure;
        set
        {
            if (value == _figure)
            {
                return;
            }

            _emptyFigure = string.IsNullOrEmpty(value);
            _figure = CleanupAvatarString(value);

            Refresh();
        }
    }

    /// @see AvatarImageWidget.as::get scale
    public string? Scale
    {
        get => _scale;
        set
        {
            if (value == _scale)
            {
                return;
            }

            _scale = value ?? "h";

            Refresh();
        }
    }

    /// @see AvatarImageWidget.as::get onlyHead
    public bool OnlyHead
    {
        get => _onlyHead;
        set
        {
            if (value == _onlyHead)
            {
                return;
            }

            _onlyHead = value;
            Refresh();
        }
    }

    /// @see AvatarImageWidget.as::get cropped
    public bool Cropped
    {
        get => _cropped;
        set
        {
            if (value == _cropped)
            {
                return;
            }

            _cropped = value;

            Refresh();
        }
    }

    /// @see AvatarImageWidget.as::get direction
    public int Direction
    {
        get => _direction;
        set
        {
            if (value == _direction)
            {
                return;
            }

            _direction = value;

            Refresh();
        }
    }

    /// @see AvatarImageWidget.as::get userId
    public int UserId
    {
        get => _userId;
        set
        {
            if (_userId == value)
            {
                return;
            }

            _userId = value;

            if (_regionWindow != null)
            {
                _regionWindow.visible = _userId > 0;
            }
        }
    }

    /// @see AvatarImageWidget.as::get properties
    public PropertyStruct[] Properties
    {
        get
        {
            if (disposed)
            {
                return System.Array.Empty<PropertyStruct>();
            }

            return
            [
                FIGURE_DEFAULT.WithValue(_figure),
                SCALE_DEFAULT.WithValue(_scale),
                ONLY_HEAD_DEFAULT.WithValue(_onlyHead),
                CROPPED_DEFAULT.WithValue(_cropped),
                DIRECTION_DEFAULT.WithValue(DIRECTIONS[_direction]),
            ];
        }
        set
        {
            foreach (PropertyStruct prop in value)
            {
                switch (prop.key)
                {
                    case FIGURE_KEY:
                        Figure = prop.value as string;
                        break;
                    case SCALE_KEY:
                        Scale = prop.value as string;
                        break;
                    case ONLY_HEAD_KEY:
                        OnlyHead = prop.value is true;
                        break;
                    case CROPPED_KEY:
                        Cropped = prop.value is true;
                        break;
                    case DIRECTION_KEY:
                        Direction = System.Array.IndexOf(DIRECTIONS, prop.value as string);
                        break;
                }
            }
        }
    }

    /// @see AvatarImageWidget.as::avatarImageReady
    public void AvatarImageReady(string param1)
    {
        if (CleanupAvatarString(param1) == _figure)
        {
            Refresh();
        }
    }

    /// @see AvatarImageWidget.as::refresh
    private void Refresh()
    {
        if (disposed || _bitmap == null || _windowManager == null)
        {
            return;
        }

        _bitmap.Bitmap = null;

        if (_windowManager.AvatarRenderer() is IAvatarRenderManager avatarRenderer)
        {
            double scaleFactor = _scale == "h" ? 1.0 : 0.5;
            string partType = _onlyHead ? "head" : "full";

            IAvatarImage? avatarImage = avatarRenderer.CreateAvatarImage(_figure, "h", null, this);
            if (avatarImage != null)
            {
                avatarImage.SetDirection(partType, _direction);

                Image? bitmapData = _cropped ? avatarImage.GetCroppedImage(partType, scaleFactor)
                    : avatarImage.GetImage(partType, true, scaleFactor);

                _bitmap.Bitmap = bitmapData;

                if (_emptyFigure)
                {
                    GreyBitmap(_bitmap);
                }

                _bitmap.DisposesBitmap = true;
                avatarImage.Dispose();
            }
        }

        // @see AvatarImageWidget.as — placeholder fallback when bitmap is null or too small
        if (_bitmap.Bitmap is not Image currentBitmap || currentBitmap.GetWidth() < 2)
        {
            string placeholderName = "placeholder_avatar"
                                     + (_scale == "sh" ? "_small" : "")
                                     + (_onlyHead ? "_head" : "")
                                     + (_cropped ? "_cropped" : "")
                                     + "_png";

            BitmapDataAsset? placeholderAsset = _windowManager.FindAssetByName(placeholderName) as BitmapDataAsset;

            if (placeholderAsset?.Content is Image placeholderImage)
            {
                _bitmap.Bitmap = placeholderImage;
                _bitmap.DisposesBitmap = false;
                GreyBitmap(_bitmap);
            }
        }

        (_bitmap as IWindow)?.Invalidate();

        currentBitmap = _bitmap.Bitmap!;

        if (currentBitmap == null || _widgetWindow == null)
        {
            return;
        }

        _widgetWindow.width = currentBitmap.GetWidth();
        _widgetWindow.height = currentBitmap.GetHeight();
    }

    /// @see AvatarImageWidget.as::greyBitmap — Godot adaptation of AS3 ColorMatrixFilter greyscale
    private static void GreyBitmap(IBitmapWrapperWindow param1)
    {
        if (param1.Bitmap is not Image image)
        {
            return;
        }

        int width = image.GetWidth();
        int height = image.GetHeight();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = image.GetPixel(x, y);
                float luminance = (float)(RC * pixel.R + GC * pixel.G + BC * pixel.B);
                image.SetPixel(x, y, new Color(luminance, luminance, luminance, pixel.A));
            }
        }
    }

    /// @see AvatarImageWidget.as::onClick
    private void OnClick(WindowEvent param1, IWindow param2)
    {
        if (_userId > 0 && _windowManager != null)
        {
            // TODO(communication): Send GetExtendedProfileMessageComposer(_userId) — composer not yet ported
        }
    }

    /// @see AvatarImageWidget.as::dispose
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
