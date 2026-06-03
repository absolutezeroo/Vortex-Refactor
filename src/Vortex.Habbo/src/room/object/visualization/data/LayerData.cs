namespace Vortex.Habbo.Room.Object.Visualization.Data;

/// <summary>
/// Per-layer render parameters including blend mode, alpha, offsets and tag.
/// </summary>
/// @see com.sulake.habbo.room.object.visualization.data.LayerData (class_3646)
public class LayerData
{
    public const string DEFAULT_TAG = "";
    public const int DEFAULT_INK = 0;
    public const int DEFAULT_ALPHA = 255;
    public const bool DEFAULT_IGNORE_MOUSE = false;
    public const int DEFAULT_X_OFFSET = 0;
    public const int DEFAULT_Y_OFFSET = 0;
    public const int DEFAULT_Z_OFFSET = 0;

    public const int INK_ADD = 1;
    public const int INK_SUBTRACT = 2;
    public const int INK_DARKEN = 3;
    public const int INK_DIFFERENCE = 4;
    public const int INK_MULTIPLY = 5;
    public const int INK_INVERT = 6;
    public const int INK_SCREEN = 7;

    public string Tag { get; set; } = "";

    public int Ink { get; set; }

    public int Alpha { get; set; } = 255;

    public bool IgnoreMouse { get; set; }

    public int XOffset { get; set; }

    public int YOffset { get; set; }

    public double ZOffset { get; set; }

    public void CopyValues(LayerData? source)
    {
        if (source == null)
        {
            return;
        }

        Tag = source.Tag;
        Ink = source.Ink;
        Alpha = source.Alpha;
        IgnoreMouse = source.IgnoreMouse;
        XOffset = source.XOffset;
        YOffset = source.YOffset;
        ZOffset = source.ZOffset;
    }
}
