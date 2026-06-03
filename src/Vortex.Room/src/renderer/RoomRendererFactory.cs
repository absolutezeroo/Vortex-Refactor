using Vortex.Core.Runtime;

namespace Vortex.Room.Renderer;

/// <summary>
/// Factory component that creates room renderer instances.
/// </summary>
/// @see com.sulake.room.renderer.RoomRendererFactory (class_2015)
public class RoomRendererFactory(IContext param1, uint param2 = 0, object? param3 = null)
    : Component(param1, param2, param3), IRoomRendererFactory
{
    public IRoomRenderer CreateRenderer()
    {
        return new RoomRenderer();
    }
}
