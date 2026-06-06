// @see core/window/iterators/ContainerIterator.as

using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Iterators;

/// @see core/window/iterators/ContainerIterator.as
public class ContainerIterator : IIterator
{
    /// @see ContainerIterator.as::ContainerIterator
    public ContainerIterator() { }

    /// @see ContainerIterator.as::ContainerIterator
    public ContainerIterator(IWindow window)
    {
        Window = window;
    }

    /// @see ContainerIterator.as::get length
    public virtual uint Length => (uint)(Window?.numChildren ?? 0);

    /// @see ContainerIterator.as::indexOf
    public virtual int IndexOf(IWindow window)
    {
        return -1;
    }

    /// @see ContainerIterator.as::setProperty — reorder-or-append via Proxy
    public virtual void SetProperty(uint index, IWindow? value)
    {
        if (Window == null || value == null)
        {
            return;
        }

        int targetIndex = (int)index;
        int currentIndex = Window.GetChildIndex(value);

        if (currentIndex == targetIndex)
        {
            return;
        }

        if (currentIndex > -1)
        {
            Window.RemoveChild(value);
        }

        Window.AddChildAt(value, targetIndex);
    }

    /// @see ContainerIterator.as — expose the backing window for iterator-target resolution
    public IWindow? Window { get; }

    /// @see ContainerIterator.as::getProperty
    public virtual object? GetProperty(params object?[] args)
    {
        return null;
    }

    /// @see ContainerIterator.as::nextNameIndex
    public virtual object? NextNameIndex(params object?[] args)
    {
        return null;
    }

    /// @see ContainerIterator.as::nextValue
    public virtual object? NextValue(params object?[] args)
    {
        return null;
    }
}
