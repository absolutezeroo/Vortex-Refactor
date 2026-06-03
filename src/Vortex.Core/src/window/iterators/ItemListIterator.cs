// @see core/window/iterators/ItemListIterator.as

using Vortex.Core.Window.Components;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Iterators;

/// @see core/window/iterators/ItemListIterator.as
public class ItemListIterator : IIterator
{
    private readonly IWindow? _iterable;

    /// @see ItemListIterator.as::ItemListIterator
    public ItemListIterator() { }

    /// @see ItemListIterator.as::ItemListIterator (controller param)
    public ItemListIterator(ItemListController param1)
    {
        _iterable = param1;
    }

    /// @see ItemListIterator.as::ItemListIterator (window param — for _container delegation)
    public ItemListIterator(IWindow param1)
    {
        _iterable = param1;
    }

    /// @see core/window/iterators/ItemListIterator.as::get length
    public virtual uint Length => 0;

    /// @see core/window/iterators/ItemListIterator.as::indexOf
    public virtual int IndexOf(IWindow window)
    {
        return -1;
    }

    /// @see core/window/iterators/ItemListIterator.as::setProperty
    public virtual void SetProperty(uint index, IWindow? value) { }

    /// @see core/window/iterators/ItemListIterator.as::getProperty
    public virtual object? GetProperty(params object?[] args)
    {
        return null;
    }

    /// @see core/window/iterators/ItemListIterator.as::nextNameIndex
    public virtual object? NextNameIndex(params object?[] args)
    {
        return null;
    }

    /// @see core/window/iterators/ItemListIterator.as::nextValue
    public virtual object? NextValue(params object?[] args)
    {
        return null;
    }
}
