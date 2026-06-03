using Vortex.Room.Messages;

namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// State-only update message with no location or direction.
/// Base for most avatar status messages.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectUpdateStateMessage
public class RoomObjectUpdateStateMessage() : RoomObjectUpdateMessage(null, null);
