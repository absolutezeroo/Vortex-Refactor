// @see com.sulake.habbo.session.events.RoomSessionDimmerPresetsEventPresetItem

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionDimmerPresetsEventPresetItem
public class RoomSessionDimmerPresetsEventPresetItem
{
    /// @see RoomSessionDimmerPresetsEventPresetItem.as::RoomSessionDimmerPresetsEventPresetItem
    public RoomSessionDimmerPresetsEventPresetItem(int id, int type, uint color, uint light)
    {
        this.id = id;
        this.type = type;
        this.color = color;
        this.light = (int)light;
    }

    /// @see RoomSessionDimmerPresetsEventPresetItem.as::get id
    public int id { get; }

    /// @see RoomSessionDimmerPresetsEventPresetItem.as::get type
    public int type { get; }

    /// @see RoomSessionDimmerPresetsEventPresetItem.as::get color
    public uint color { get; }

    /// @see RoomSessionDimmerPresetsEventPresetItem.as::get light (returns int per source)
    public int light { get; }
}
