using Godot;

using Vortex.Bundle.Data;

namespace Vortex.Core.Assets.Bundle;

/// <summary>
/// Holds a loaded spritesheet Image and its frame metadata.
/// Provides lazy sub-image extraction by frame name.
/// </summary>
public sealed class VortexSpritesheet : System.IDisposable
{
    private readonly Image _atlas;
    private readonly Dictionary<string, FrameData> _framesByName;
    private readonly Dictionary<string, Image> _cache = new();
    private readonly StringTable _strings;

    public VortexSpritesheet(Image atlas, SpritesheetMeta meta, StringTable strings)
    {
        _atlas = atlas;
        _strings = strings;
        _framesByName = new Dictionary<string, FrameData>(meta.Frames.Length);

        foreach (FrameData frame in meta.Frames)
        {
            string? name = strings.Resolve(frame.NameIndex);

            if (name != null)
            {
                _framesByName[name] = frame;
            }
        }
    }

    /// <summary>
    /// Extracts a sub-image for the given frame name.
    /// Returns the trimmed sprite image (not the full source size).
    /// Caches results for repeated access.
    /// </summary>
    public Image? ExtractFrame(string name)
    {
        if (_cache.TryGetValue(name, out Image? cached))
        {
            return cached;
        }

        if (!_framesByName.TryGetValue(name, out FrameData frame))
        {
            return null;
        }

        if (frame.Width == 0 || frame.Height == 0)
        {
            return null;
        }

        // Extract sub-image from atlas
        Image? subImage = Image.CreateEmpty(frame.Width, frame.Height, false, Image.Format.Rgba8);
        subImage.BlitRect(
            _atlas,
            new Rect2I(frame.X, frame.Y, frame.Width, frame.Height),
            new Vector2I(0, 0)
        );

        _cache[name] = subImage;

        return subImage;
    }

    /// <summary>
    /// Gets the frame metadata for a sprite name (trim offsets, source size).
    /// </summary>
    public FrameData? GetFrameData(string name)
    {
        return _framesByName.TryGetValue(name, out FrameData frame) ? frame : null;
    }

    public bool HasFrame(string name)
    {
        return _framesByName.ContainsKey(name);
    }

    public int FrameCount => _framesByName.Count;

    public void Dispose()
    {
        _cache.Clear();
        // Godot Images are RefCounted — they'll be freed by GC
    }
}
