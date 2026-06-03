// @see core/window/iterators/EmptyIterator.as

using Vortex.Core.Window.Components;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Iterators;

/// <summary>
/// Singleton empty iterator. Returns 0 length, -1 for indexOf, null for values.
/// </summary>
/// @see core/window/iterators/EmptyIterator.as
public class EmptyIterator : IIterator
{
    public static readonly EmptyIterator INSTANCE = new();

    /// @see EmptyIterator.as::get length
    public uint Length => 0;

    /// @see EmptyIterator.as::indexOf
    public int IndexOf(IWindow window)
    {
        return -1;
    }

    /// @see EmptyIterator.as::getProperty
    object? IIterator.GetProperty(params object?[] args)
    {
        return null;
    }

    /// @see EmptyIterator.as::setProperty
    public void SetProperty(uint index, IWindow? value) { }

    /// @see EmptyIterator.as::nextNameIndex
    public static int NextNameIndex(int index)
    {
        return 0;
    }

    /// @see EmptyIterator.as::nextValue
    public static object? NextValue(int index)
    {
        return null;
    }
}
