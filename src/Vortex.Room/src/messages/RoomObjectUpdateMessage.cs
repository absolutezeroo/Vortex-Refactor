using Vortex.Room.Utils;

namespace Vortex.Room.Messages;

/// <summary>
/// Base update message with optional location and direction vectors.
/// </summary>
/// @see com.sulake.room.messages.RoomObjectUpdateMessage
public class RoomObjectUpdateMessage(IVector3d? location, IVector3d? direction)
{
    public IVector3d? Location { get; } = location;
    public IVector3d? Direction { get; } = direction;
}
