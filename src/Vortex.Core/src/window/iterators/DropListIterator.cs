// @see core/window/iterators/DropListIterator.as

using Vortex.Core.Window.Components;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Iterators;

/// @see core/window/iterators/DropListIterator.as
public class DropListIterator : IIterator
{
    private readonly IWindow? _window;

    /// @see DropListIterator.as::DropListIterator
    public DropListIterator() { }

    /// @see DropListIterator.as::DropListIterator
    public DropListIterator(IWindow window)
    {
        _window = window;
    }

    /// @see DropListIterator.as::get length
    public virtual uint Length => (uint)(_window?.numChildren ?? 0);

    /// @see DropListIterator.as::indexOf
    public virtual int IndexOf(IWindow window)
    {
        return -1;
    }

    /// @see DropListIterator.as::setProperty
    public virtual void SetProperty(uint index, IWindow? value) { }

    /// @see DropListIterator.as::getProperty
    public virtual object? GetProperty(params object?[] args)
    {
        return null;
    }

    /// @see DropListIterator.as::nextNameIndex
    public virtual object? NextNameIndex(params object?[] args)
    {
        return null;
    }

    /// @see DropListIterator.as::nextValue
    public virtual object? NextValue(params object?[] args)
    {
        return null;
    }
}
