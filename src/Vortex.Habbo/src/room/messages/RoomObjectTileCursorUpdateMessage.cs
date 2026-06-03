using Vortex.Room.Messages;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Tile cursor position and visibility update for mouse hover highlighting.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectTileCursorUpdateMessage
public class RoomObjectTileCursorUpdateMessage
(
    IVector3d? location,
    double height,
    bool visible,
    string sourceEventId,
    bool toggleVisibility = false
)
    : RoomObjectUpdateMessage(location, null)
{
    public double Height => height;
    public bool Visible => visible;
    public string SourceEventId => sourceEventId;
    public bool ToggleVisibility => toggleVisibility;
}
