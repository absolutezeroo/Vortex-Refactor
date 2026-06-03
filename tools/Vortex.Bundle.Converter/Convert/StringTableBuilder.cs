namespace Vortex.Bundle.Converter.Convert;

/// <summary>
/// Collects unique strings and assigns stable indices for the .vortex string table.
/// Thread-safe for single-threaded build pipelines (no concurrent access expected).
/// </summary>
public sealed class StringTableBuilder
{
    private readonly Dictionary<string, uint> _indexMap = new();
    private readonly List<string> _strings = [];

    /// <summary>
    /// Adds a string to the table. Returns its index.
    /// If already present, returns the existing index (deduplication).
    /// </summary>
    public uint Add(string value)
    {
        if (_indexMap.TryGetValue(value, out uint existing))
        {
            return existing;
        }

        uint index = (uint)_strings.Count;
        _strings.Add(value);
        _indexMap[value] = index;
        return index;
    }

    /// <summary>
    /// Returns the complete string table as an array, ready for serialization.
    /// </summary>
    public string[] ToArray()
    {
        return _strings.ToArray();
    }

    public int Count => _strings.Count;
}