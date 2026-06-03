// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/events/LibraryLoadedEvent.as

namespace Vortex.Habbo.Avatar.Events;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/events/LibraryLoadedEvent.as
public class LibraryLoadedEvent
{
    public const string LIBRARY_LOADED = "LIBRARY_LOADED";

    /// @see LibraryLoadedEvent.as::LibraryLoadedEvent
    public LibraryLoadedEvent(string type, string library)
    {
        Type = type;
        Library = library;
    }

    public string Type { get; }

    /// @see LibraryLoadedEvent.as::get library
    public string Library { get; }
}
