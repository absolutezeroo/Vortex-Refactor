// @see core/window/utils/IIterator.as

namespace Vortex.Core.Window.Utils;

/// @see core/window/utils/IIterator.as — AS3 iterators extend Proxy; this interface captures the Proxy methods
public interface IIterator
{
    /// @see core/window/utils/IIterator.as::get length
    uint Length { get; }

    /// @see core/window/utils/IIterator.as::indexOf
    int IndexOf(IWindow window);

    /// @see flash.utils.Proxy::setProperty — AS3 `iterator[index] = value` routes here
    void SetProperty(uint index, IWindow? value);

    /// @see flash.utils.Proxy::getProperty — AS3 `iterator[index]` routes here
    object? GetProperty(params object?[] args);
}
