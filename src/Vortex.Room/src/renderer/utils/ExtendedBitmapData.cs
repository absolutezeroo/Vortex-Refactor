using Godot;

namespace Vortex.Room.Renderer.Utils;

/// <summary>
/// Reference-counted Image wrapper. Disposes only when all references are released.
/// </summary>
/// @see com.sulake.room.renderer.utils.ExtendedBitmapData (class_3815)
public class ExtendedBitmapData
{
    public Image? Data { get; private set; }
    public int Width { get; }
    public int Height { get; }

    public ExtendedBitmapData(int width, int height, bool transparent = true, uint fillColor = 0)
    {
        Width = width;
        Height = height;
        Data = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        if (fillColor != 0)
        {
            float r = ((fillColor >> 16) & 0xFF) / 255f;
            float g = ((fillColor >> 8) & 0xFF) / 255f;
            float b = (fillColor & 0xFF) / 255f;
            float a = transparent ? ((fillColor >> 24) & 0xFF) / 255f : 1f;
            Data.Fill(new Color(r, g, b, a));
        }
    }

    public int ReferenceCount { get; private set; }

    public bool Disposed { get; private set; }

    public void AddReference()
    {
        ReferenceCount++;
    }

    public void Dispose()
    {
        if (Disposed)
        {
            return;
        }
        if (--ReferenceCount <= 0)
        {
            Data = null;
            Disposed = true;
        }
    }

    public ExtendedBitmapData Clone()
    {
        ExtendedBitmapData clone = new(Width, Height, true, 0xFFFFFF);
        if (Data != null && clone.Data != null)
        {
            clone.Data.BlitRect(Data, new Rect2I(0, 0, Width, Height), Vector2I.Zero);
        }
        return clone;
    }
}
