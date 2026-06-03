namespace Vortex.Bundle.Data;

/// <summary>
/// Table of contents entry (12 bytes on disk).
/// Maps a section ID to its offset and length in the bundle file.
/// </summary>
public struct TocEntry
{
    public ushort SectionId;
    public ushort Reserved;
    public uint Offset;
    public uint Length;
}
