namespace Vortex.Bundle.Converter.Swf;

/// <summary>
/// Represents a parsed SWF file: header metadata and extracted tags.
/// </summary>
public sealed class SwfFile
{
    public byte Version { get; set; }
    public uint FileLength { get; set; }
    public SwfRect FrameSize { get; set; }
    public float FrameRate { get; set; }
    public ushort FrameCount { get; set; }

    public List<SwfTag> Tags { get; set; } = [];
}

public struct SwfRect
{
    public int XMin;
    public int XMax;
    public int YMin;
    public int YMax;

    public int Width => (XMax - XMin) / 20;
    public int Height => (YMax - YMin) / 20;
}

/// <summary>
/// A generic SWF tag with code and raw data.
/// </summary>
public class SwfTag
{
    public ushort Code { get; set; }
    public byte[] Data { get; set; } = [];
}

/// <summary>
/// Tag 76: SymbolClass — maps character IDs to class names.
/// </summary>
public sealed class SymbolClassTag : SwfTag
{
    public List<(ushort CharId, string ClassName)> Symbols { get; set; } = [];
}

/// <summary>
/// Tag 87: DefineBinaryData — embedded binary blobs (XML, etc).
/// </summary>
public sealed class DefineBinaryDataTag : SwfTag
{
    public ushort CharacterId { get; set; }
    public string? ClassName { get; set; }
    public byte[] BinaryData { get; set; } = [];
}

/// <summary>
/// Tags 20/36: DefineBitsLossless / DefineBitsLossless2.
/// </summary>
public sealed class DefineBitsLosslessTag : SwfTag
{
    public ushort CharacterId { get; set; }
    public string? ClassName { get; set; }
    public byte BitmapFormat { get; set; }
    public ushort BitmapWidth { get; set; }
    public ushort BitmapHeight { get; set; }
    public byte BitmapColorTableSize { get; set; }
    public byte[] ZlibBitmapData { get; set; } = [];
    public bool HasAlpha { get; set; } // true for tag 36
}

/// <summary>
/// Tags 21/35/90: DefineBitsJPEG2/3/4.
/// </summary>
public sealed class DefineBitsJpegTag : SwfTag
{
    public ushort CharacterId { get; set; }
    public string? ClassName { get; set; }
    public byte[] ImageData { get; set; } = [];
    public byte[] AlphaData { get; set; } = [];
    public bool HasAlpha { get; set; } // true for tags 35/90
}