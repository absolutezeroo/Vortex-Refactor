// @see core/window/iterators/ItemGridIterator.as

using Vortex.Core.Window.Components;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Iterators;

/// @see core/window/iterators/ItemGridIterator.as
public class ItemGridIterator : IIterator
{
    private readonly ItemGridController? _iterable;

    /// @see ItemGridIterator.as::ItemGridIterator
    public ItemGridIterator() { }

    /// @see ItemGridIterator.as::ItemGridIterator (controller param)
    public ItemGridIterator(ItemGridController param1)
    {
        _iterable = param1;
    }

    /// @see core/window/iterators/ItemGridIterator.as::get length
    public virtual uint Length => 0;

    /// @see core/window/iterators/ItemGridIterator.as::indexOf
    public virtual int IndexOf(IWindow window)
    {
        return -1;
    }

    /// @see core/window/iterators/ItemGridIterator.as::setProperty
    public virtual void SetProperty(uint index, IWindow? value) { }

    /// @see core/window/iterators/ItemGridIterator.as::getProperty
    public virtual object? GetProperty(params object?[] args)
    {
        return null;
    }

    /// @see core/window/iterators/ItemGridIterator.as::nextNameIndex
    public virtual object? NextNameIndex(params object?[] args)
    {
        return null;
    }

    /// @see core/window/iterators/ItemGridIterator.as::nextValue
    public virtual object? NextValue(params object?[] args)
    {
        return null;
    }
}
