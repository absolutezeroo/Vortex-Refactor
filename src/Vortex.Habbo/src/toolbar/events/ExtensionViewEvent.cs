namespace Vortex.Habbo.Toolbar.Events;

/// @see com.sulake.habbo.toolbar.events.ExtensionViewEvent
public class ExtensionViewEvent
{
    /// @see ExtensionViewEvent.as::const_575
    public const string EXTENSION_VIEW_RESIZED = "EVE_EXTENSION_VIEW_RESIZED";

    public string type { get; }

    public ExtensionViewEvent(string type)
    {
        this.type = type;
    }
}
