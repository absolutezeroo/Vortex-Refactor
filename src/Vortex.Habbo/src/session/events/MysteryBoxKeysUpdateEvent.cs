// @see com.sulake.habbo.session.events.MysteryBoxKeysUpdateEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.MysteryBoxKeysUpdateEvent
public class MysteryBoxKeysUpdateEvent
{
    public const string MYSTERY_BOX_KEYS_UPDATE = "MysteryBoxKeysUpdateEvent";

    public MysteryBoxKeysUpdateEvent(string boxColor, string keyColor)
    {
        BoxColor = boxColor;
        KeyColor = keyColor;
        Type = MYSTERY_BOX_KEYS_UPDATE;
    }

    public string Type { get; }
    public string BoxColor { get; }
    public string KeyColor { get; }
}
