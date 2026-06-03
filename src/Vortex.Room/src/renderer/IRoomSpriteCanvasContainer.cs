using Vortex.Room.Object;

namespace Vortex.Room.Renderer;

/// <summary>
/// Container interface for the room renderer, providing access to room objects
/// that the canvas iterates during rendering.
/// </summary>
/// @see com.sulake.room.renderer.IRoomSpriteCanvasContainer (class_3446)
public interface IRoomSpriteCanvasContainer
{
    string? RoomObjectVariableAccurateZ { get; }
    IRoomObject? GetRoomObject(string identifier);
    IRoomObject? GetRoomObjectWithIndex(int index);
    string? GetRoomObjectIdWithIndex(int index);
    int GetRoomObjectCount();
    string? GetRoomObjectIdentifier(IRoomObject obj);
}
