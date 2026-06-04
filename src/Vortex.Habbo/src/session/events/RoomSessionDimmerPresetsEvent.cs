// @see com.sulake.habbo.session.events.RoomSessionDimmerPresetsEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionDimmerPresetsEvent
public class RoomSessionDimmerPresetsEvent : RoomSessionEvent
{
    public const string ROOM_DIMMER_PRESETS = "RSDPE_PRESETS";

    private readonly List<RoomSessionDimmerPresetsEventPresetItem?> _presets = [];

    /// @see RoomSessionDimmerPresetsEvent.as::RoomSessionDimmerPresetsEvent
    public RoomSessionDimmerPresetsEvent(string type, IRoomSession session, bool openLandingPage = false)
        : base(type, session, openLandingPage)
    {
    }

    /// @see RoomSessionDimmerPresetsEvent.as::get selectedPresetId
    public int selectedPresetId { get; set; } = 0;

    /// @see RoomSessionDimmerPresetsEvent.as::get presetCount
    public int presetCount => _presets.Count;

    /// @see RoomSessionDimmerPresetsEvent.as::storePreset
    public void StorePreset(int id, int type, int color, int light)
    {
        int index = id - 1;
        while (_presets.Count <= index)
        {
            _presets.Add(null);
        }

        _presets[index] = new RoomSessionDimmerPresetsEventPresetItem(id, type, (uint)color, (uint)light);
    }

    /// @see RoomSessionDimmerPresetsEvent.as::getPreset
    public RoomSessionDimmerPresetsEventPresetItem? GetPreset(int index)
    {
        if (index < 0 || index >= _presets.Count)
        {
            return null;
        }

        return _presets[index];
    }
}
