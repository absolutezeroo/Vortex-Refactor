using Godot;

namespace Vortex.Room.Renderer.Utils;

/// <summary>
/// Extended sprite display object with reference-counted bitmap data and hit testing.
/// Replaces Flash's Bitmap display child in the room canvas.
/// </summary>
/// @see com.sulake.room.renderer.utils.ExtendedSprite (class_3741)
public class ExtendedSprite
{
    private ExtendedBitmapData? _bitmapData;
    private int _width;
    private int _height;
    private int _updateId1 = -1;
    private int _updateId2 = -1;

    public int X { get; set; }
    public int Y { get; set; }
    public double Alpha { get; set; } = 1;
    public string BlendMode { get; set; } = "normal";

    /// <summary>
    /// Godot Sprite2D node that visually represents this sprite in the scene tree.
    /// Mirrors the AS3 pattern where each ExtendedSprite (Bitmap) was a child of the display Sprite.
    /// </summary>
    public Sprite2D? DisplaySprite { get; set; }

    /// <summary>
    /// Cached ImageTexture to avoid recreating every frame. Uses Update() when image size unchanged.
    /// </summary>
    public ImageTexture? DisplayTexture { get; set; }

    public int AlphaTolerance { get; set; } = 128;

    public string Tag { get; set; } = "";
    public string Identifier { get; set; } = "";
    public bool VaryingDepth { get; set; }
    public bool ClickHandling { get; set; }
    public bool SkipMouseHandling { get; set; }
    public int OffsetRefX { get; set; }
    public int OffsetRefY { get; set; }

    public ExtendedBitmapData? BitmapData
    {
        get => _bitmapData;
        set
        {
            if (value == _bitmapData)
            {
                return;
            }
            _bitmapData?.Dispose();
            _bitmapData = null;
            if (value != null)
            {
                _width = value.Width;
                _height = value.Height;
                value.AddReference();
                _bitmapData = value;
            }
            else
            {
                _width = 0;
                _height = 0;
                _updateId1 = -1;
                _updateId2 = -1;
            }
        }
    }

    public bool NeedsUpdate(int updateId1, int updateId2)
    {
        if (updateId1 != _updateId1 || updateId2 != _updateId2)
        {
            _updateId1 = updateId1;
            _updateId2 = updateId2;
            return true;
        }
        if (_bitmapData is { Disposed: true })
        {
            return true;
        }
        return false;
    }

    public bool HitTest(int x, int y)
    {
        if (AlphaTolerance > 255 || _bitmapData == null)
        {
            return false;
        }
        if (x < 0 || y < 0 || x >= _width || y >= _height)
        {
            return false;
        }
        return HitTestBitmapData(x, y);
    }

    private bool HitTestBitmapData(int x, int y)
    {
        try
        {
            Color pixel = _bitmapData!.Data!.GetPixel(x, y);
            int alpha = (int)(pixel.A * 255);
            return alpha > AlphaTolerance;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _bitmapData?.Dispose();
        _bitmapData = null;
        DisplayTexture = null;
        DisplaySprite?.QueueFree();
        DisplaySprite = null;
    }
}
