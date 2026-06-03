// @see com.sulake.habbo.session.events.PerksUpdatedEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.PerksUpdatedEvent
public class PerksUpdatedEvent
{
    public const string PERKS_UPDATED = "PerksUpdatedEvent";
    public string Type { get; } = PERKS_UPDATED;
}
