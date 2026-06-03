namespace Vortex.Bundle.Data;

/// <summary>
/// A single sprite frame within the spritesheet (20 bytes on disk).
/// Describes where the frame lives in the atlas and its original dimensions.
/// </summary>
public struct FrameData
{
    /// <summary>String table index for the frame/sprite name.</summary>
    public uint NameIndex;

    /// <summary>X position in the atlas.</summary>
    public ushort X;

    /// <summary>Y position in the atlas.</summary>
    public ushort Y;

    /// <summary>Width in the atlas (after trim).</summary>
    public ushort Width;

    /// <summary>Height in the atlas (after trim).</summary>
    public ushort Height;

    /// <summary>Original source width before trim.</summary>
    public ushort SourceWidth;

    /// <summary>Original source height before trim.</summary>
    public ushort SourceHeight;

    /// <summary>Trim offset X (pixels removed from left).</summary>
    public short TrimX;

    /// <summary>Trim offset Y (pixels removed from top).</summary>
    public short TrimY;
}
