// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/cache/ImageData.as

using Godot;

namespace Vortex.Habbo.Avatar.Cache;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/cache/ImageData.as
public class ImageData
{
    private Vector2I _regPoint;

    /// @see ImageData.as::ImageData
    public ImageData(Image? bitmap, Rect2I rect, Vector2I regPoint, bool flipH, float[]? colorTransform)
    {
        Bitmap = bitmap;
        Rect = rect;
        _regPoint = regPoint;
        FlipH = flipH;
        ColorTransform = colorTransform;

        if (flipH)
        {
            _regPoint = new Vector2I(-_regPoint.X + rect.Size.X, _regPoint.Y);
        }
    }

    /// @see ImageData.as::dispose
    public void Dispose()
    {
        Bitmap = null;
    }

    /// @see ImageData.as::get bitmap
    public Image? Bitmap { get; private set; }

    /// @see ImageData.as::get rect
    public Rect2I Rect { get; }

    /// @see ImageData.as::get regPoint
    public Vector2I RegPoint => _regPoint;

    /// @see ImageData.as::get flipH
    public bool FlipH { get; }

    /// @see ImageData.as::get colorTransform
    public float[]? ColorTransform { get; }

    /// @see ImageData.as::get offsetRect
    public Rect2I OffsetRect => new(
        new Vector2I(-_regPoint.X, -_regPoint.Y),
        Rect.Size
    );
}
