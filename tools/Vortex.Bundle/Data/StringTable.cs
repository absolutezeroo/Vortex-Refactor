namespace Vortex.Bundle.Data;

/// <summary>
/// Read-only string table loaded from a .vortex bundle.
/// All other sections reference strings by uint32 index into this table.
/// </summary>
public sealed class StringTable(string[] entries)
{
    public int Count => entries.Length;

    /// <summary>
    /// Resolves a string index. Returns null for <see cref="VortexBundleFormat.NULL_STRING"/>.
    /// </summary>
    public string? Resolve(uint index)
    {
        if (index == VortexBundleFormat.NULL_STRING)
        {
            return null;
        }
        if (index >= (uint)entries.Length)
        {
            throw new IndexOutOfRangeException($"String index {index} out of range (count={entries.Length}).");
        }
        return entries[index];
    }

    /// <summary>Returns the string at the given index (must be valid, non-null).</summary>
    public string this[uint index] => entries[index];
}
