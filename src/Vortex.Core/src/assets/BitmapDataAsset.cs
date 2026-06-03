// @see core/assets/BitmapDataAsset.as

using System;
using System.Xml.Linq;

using Godot;

namespace Vortex.Core.Assets;

/// @see core/assets/BitmapDataAsset.as
/// Godot adaptation: wraps a Godot Image instead of AS3 BitmapData.
public class BitmapDataAsset : ILazyAsset
{
    /// @see BitmapDataAsset.as::var_214
    protected static uint _instances;

    /// @see BitmapDataAsset.as::var_212
    protected static uint _allocatedByteCount;

    private Rect2? _rectangle;
    private object? _unknownContent;
    private Image? _bitmap;

    /// @see BitmapDataAsset.as::var_1034
    private readonly bool _ownsContent = true;

    /// @see BitmapDataAsset.as::name
    public string? Name { get; set; }

    /// @see BitmapDataAsset.as::BitmapDataAsset
    public BitmapDataAsset(AssetTypeDeclaration? declaration = null, string? url = null)
    {
        Declaration = declaration;
        Url = url;
        _instances++;
    }

    /// Backward-compatible constructor for existing code that passes an Image directly.
    public BitmapDataAsset(Image content) : this(null, null)
    {
        _bitmap = content;

        if (content != null)
        {
            _rectangle = new Rect2(0, 0, content.GetWidth(), content.GetHeight());
        }
    }

    /// @see BitmapDataAsset.as::get instances
    public static uint Instances => _instances;

    /// @see BitmapDataAsset.as::get allocatedByteCount
    public static uint AllocatedByteCount => _allocatedByteCount;

    /// @see BitmapDataAsset.as::get url
    public string? Url { get; set; }

    /// @see BitmapDataAsset.as::get flipH
    public bool FlipH { get; private set; }

    /// @see BitmapDataAsset.as::get flipV
    public bool FlipV { get; private set; }

    /// @see BitmapDataAsset.as::get offset
    public Vector2 Offset { get; private set; } = Vector2.Zero;

    /// @see BitmapDataAsset.as::get content
    public object? Content
    {
        get
        {
            if (_bitmap == null)
            {
                PrepareLazyContent();
            }
            return _bitmap;
        }
    }

    /// @see BitmapDataAsset.as::get disposed
    public bool disposed { get; private set; }

    /// @see BitmapDataAsset.as::get rectangle
    public Rect2? Rectangle
    {
        get
        {
            if (_rectangle != null)
            {
                return _rectangle;
            }

            if (Content is Image image)
            {
                _rectangle = new Rect2(0, 0, image.GetWidth(), image.GetHeight());
            }

            return _rectangle;
        }
    }

    /// @see BitmapDataAsset.as::get declaration
    public AssetTypeDeclaration? Declaration { get; private set; }

    /// @see BitmapDataAsset.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        _instances--;

        if (_bitmap != null)
        {
            try
            {
                _allocatedByteCount -= (uint)(_bitmap.GetWidth() * _bitmap.GetHeight() * 4);
            }
            catch
            {
                // Ignore dispose errors
            }
        }

        _unknownContent = null;
        _bitmap = null;
        Offset = Vector2.Zero;
        Declaration = null;
        Url = null;
        _rectangle = null;
        disposed = true;
    }

    /// @see BitmapDataAsset.as::setUnknownContent
    public void SetUnknownContent(object? content)
    {
        if (content == null)
        {
            return;
        }

        if (_bitmap != null && ReferenceEquals(_bitmap, content))
        {
            return;
        }

        _unknownContent = content;
        _bitmap = null;
    }

    /// @see BitmapDataAsset.as::prepareLazyContent
    /// Godot adaptation: handles Image, byte[] (PNG), BitmapDataAsset sources.
    /// AS3 Class/Bitmap types are not applicable in Godot.
    public void PrepareLazyContent()
    {
        switch (_unknownContent)
        {
            case null:
                return;
            case Image image:
                _bitmap = image;
                _unknownContent = null;
                return;
            case BitmapDataAsset otherAsset:
                {
                    _bitmap = otherAsset._bitmap;
                    Offset = otherAsset.Offset;
                    FlipH = otherAsset.FlipH;
                    FlipV = otherAsset.FlipV;

                    if (_bitmap == null)
                    {
                        throw new InvalidOperationException("Failed to read content from BitmapDataAsset!");
                    }

                    _unknownContent = null;

                    return;
                }
            case byte[] bytes:
                try
                {
                    Image img = new();
                    Error error = img.LoadPngFromBuffer(bytes);

                    if (error == Error.Ok)
                    {
                        _bitmap = img;
                    }
                    else
                    {
                        // Try JPEG
                        error = img.LoadJpgFromBuffer(bytes);

                        if (error == Error.Ok)
                        {
                            _bitmap = img;
                        }
                    }
                }
                catch (Exception)
                {
                    Logger.Error($"[BitmapDataAsset] Error decoding asset content: {Url}::{Name}");
                }

                _unknownContent = null;

                return;
            default:
                _unknownContent = null;
                break;
        }

    }

    /// @see BitmapDataAsset.as::setFromOtherAsset
    public void SetFromOtherAsset(IAsset param1)
    {
        if (param1 is not BitmapDataAsset other)
        {
            throw new InvalidOperationException("Provided asset should be of type BitmapDataAsset!");
        }

        _bitmap = other._bitmap;
        Offset = other.Offset;
    }

    /// @see BitmapDataAsset.as::setParamsDesc
    public void SetParamsDesc(IEnumerable<XElement>? param1)
    {
        if (param1 == null)
        {
            return;
        }

        foreach (XElement element in param1)
        {
            string? key = element.Attribute("key")?.Value;
            string? value = element.Attribute("value")?.Value;

            if (key == null || value == null)
            {
                continue;
            }

            switch (key)
            {
                case "offset":
                    {
                        string[] parts = value.Split(',');
                        if (parts.Length >= 2)
                        {
                            Offset = new Vector2(
                                int.TryParse(parts[0], out int x) ? x : 0,
                                int.TryParse(parts[1], out int y) ? y : 0
                            );
                        }
                        break;
                    }
                case "region":
                    {
                        string[] parts = value.Split(',');

                        if (parts.Length >= 4)
                        {
                            _rectangle = new Rect2(
                                int.TryParse(parts[0], out int rx) ? rx : 0,
                                int.TryParse(parts[1], out int ry) ? ry : 0,
                                int.TryParse(parts[2], out int rw) ? rw : 0,
                                int.TryParse(parts[3], out int rh) ? rh : 0
                            );
                        }
                        break;
                    }
                case "flipH":
                    FlipH = value is "1" or "true";
                    break;
                case "flipV":
                    FlipV = value is "1" or "true";
                    break;
            }
        }
    }
}
