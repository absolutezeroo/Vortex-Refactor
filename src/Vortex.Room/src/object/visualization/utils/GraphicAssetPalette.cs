using Godot;

namespace Vortex.Room.Object.Visualization.Utils;

/// @see com.sulake.room.object.visualization.utils.GraphicAssetPalette
public class GraphicAssetPalette
{
    private static readonly int[] s_blank = new int[256];

    private int[] _palette;

    public int PrimaryColor { get; }
    public int SecondaryColor { get; }

    public GraphicAssetPalette(byte[] data, int primaryColor, int secondaryColor)
    {
        _palette = new int[256];
        int offset = 0;
        int index = 0;
        while (offset + 2 < data.Length && index < 256)
        {
            byte r = data[offset];
            byte g = data[offset + 1];
            byte b = data[offset + 2];
            _palette[index] = unchecked((int)(0xFF000000u | ((uint)r << 16) | ((uint)g << 8) | b));
            offset += 3;
            index++;
        }
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
    }

    public void Dispose()
    {
        _palette = [];
    }

    /// <summary>
    /// Colorizes a bitmap by remapping the green channel through the palette.
    /// AS3 equivalent: paletteMap on green channel, then restore original alpha.
    /// </summary>
    public void ColorizeBitmap(Image image)
    {
        int width = image.GetWidth();
        int height = image.GetHeight();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = image.GetPixel(x, y);
                float originalAlpha = pixel.A;
                int greenIndex = (int)(pixel.G * 255) & 0xFF;
                int paletteColor = _palette[greenIndex];
                float r = ((paletteColor >> 16) & 0xFF) / 255f;
                float g = ((paletteColor >> 8) & 0xFF) / 255f;
                float b = (paletteColor & 0xFF) / 255f;
                image.SetPixel(x, y, new Color(r, g, b, originalAlpha));
            }
        }
    }
}
