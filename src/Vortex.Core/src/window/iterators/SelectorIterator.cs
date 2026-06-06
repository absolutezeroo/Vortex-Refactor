// @see core/window/iterators/SelectorIterator.as

using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Iterators;

/// @see core/window/iterators/SelectorIterator.as
public class SelectorIterator : IIterator
{
    private readonly IWindow? _window;

    /// @see SelectorIterator.as::SelectorIterator
    public SelectorIterator() { }

    /// @see SelectorIterator.as::SelectorIterator
    public SelectorIterator(IWindow window)
    {
        _window = window;
    }

    /// @see SelectorIterator.as::get length
    public virtual uint Length => (uint)(_window?.numChildren ?? 0);

    /// @see SelectorIterator.as::indexOf
    public virtual int IndexOf(IWindow window)
    {
        return -1;
    }

    /// @see SelectorIterator.as::setProperty
    public virtual void SetProperty(uint index, IWindow? value) { }

    /// @see SelectorIterator.as::getProperty
    public virtual object? GetProperty(params object?[] args)
    {
        return null;
    }

    /// @see SelectorIterator.as::nextNameIndex
    public virtual object? NextNameIndex(params object?[] args)
    {
        return null;
    }

    /// @see SelectorIterator.as::nextValue
    public virtual object? NextValue(params object?[] args)
    {
        return null;
    }
}
