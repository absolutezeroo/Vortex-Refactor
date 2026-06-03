// @see core/window/utils/IEventQueue.as

namespace Vortex.Core.Window.Utils;

/// @see core/window/utils/IEventQueue.as
public interface IEventQueue
{
    /// @see core/window/utils/IEventQueue.as::get length
    uint Length { get; }

    /// @see core/window/utils/IEventQueue.as::begin
    void Begin();

    /// @see core/window/utils/IEventQueue.as::next
    object? Next();

    /// @see core/window/utils/IEventQueue.as::remove
    void Remove();

    /// @see core/window/utils/IEventQueue.as::end
    void End();

    /// @see core/window/utils/IEventQueue.as::flush
    void Flush();
}
