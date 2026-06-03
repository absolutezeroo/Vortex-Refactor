// @see core/utils/profiler/tracking/TrackedBitmapData.as

using System;

using Godot;

namespace Vortex.Core.Window.Graphics;

/// <summary>
/// Wraps Godot Image with memory tracking, equivalent to AS3 TrackedBitmapData.
/// </summary>
/// @see core/utils/profiler/tracking/TrackedBitmapData.as
public class TrackedImage : IDisposable
{
    private const int MAX_PIXELS = 16777215;
    private const int MAX_WIDTH = 8191;
    private const int MAX_HEIGHT = 8191;
    private const int MIN_WIDTH = 1;
    private const int MIN_HEIGHT = 1;
    public const int DEFAULT_SIZE = 4095;

    private bool _disposed;

    public Image ImageData { get; private set; }
    public int Width { get; }
    public int Height { get; }
    public bool Transparent { get; }

    public static uint NumInstances { get; private set; }

    public static long AllocatedByteCount { get; private set; }

    /// @see TrackedBitmapData.as::TrackedBitmapData
    public TrackedImage(int width, int height, bool transparent = true, uint fillColor = 0xFFFFFFFF)
    {
        if ((long)width * height > MAX_PIXELS)
        {
            width = DEFAULT_SIZE;
            height = DEFAULT_SIZE;
        }
        else
        {
            width = Math.Clamp(width, MIN_WIDTH, MAX_WIDTH);
            height = Math.Clamp(height, MIN_HEIGHT, MAX_HEIGHT);
        }

        Width = width;
        Height = height;
        Transparent = transparent;
        ImageData = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);

        byte a = (byte)((fillColor >> 24) & 0xFF);
        byte r = (byte)((fillColor >> 16) & 0xFF);
        byte g = (byte)((fillColor >> 8) & 0xFF);
        byte b = (byte)(fillColor & 0xFF);
        ImageData.Fill(new Color(r / 255f, g / 255f, b / 255f, a / 255f));

        NumInstances++;
        AllocatedByteCount += (long)width * height * 4;
    }

    /// @see TrackedBitmapData.as::dispose
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        AllocatedByteCount -= (long)Width * Height * 4;
        NumInstances--;
        _disposed = true;
        ImageData = null!;
    }

    /// @see TrackedBitmapData.as::clone
    public TrackedImage Clone()
    {
        if (_disposed)
        {
            return null!;
        }

        TrackedImage clone = new(Width, Height, Transparent);

        clone.ImageData.BlitRect(ImageData, new Rect2I(0, 0, Width, Height), Vector2I.Zero);

        return clone;
    }
}
