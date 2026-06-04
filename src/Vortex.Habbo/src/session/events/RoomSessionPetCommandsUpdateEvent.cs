// @see com.sulake.habbo.session.events.RoomSessionPetCommandsUpdateEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionPetCommandsUpdateEvent
public class RoomSessionPetCommandsUpdateEvent : RoomSessionEvent
{
    public const string PET_COMMANDS = "RSPIUE_ENABLED_PET_COMMANDS";

    /// @see RoomSessionPetCommandsUpdateEvent.as::RoomSessionPetCommandsUpdateEvent
    public RoomSessionPetCommandsUpdateEvent(IRoomSession session, int petId,
        IReadOnlyList<int> allCommands, IReadOnlyList<int> enabledCommands, bool openLandingPage = false)
        : base(PET_COMMANDS, session, openLandingPage)
    {
        this.petId = petId;
        this.allCommands = allCommands;
        this.enabledCommands = enabledCommands;
    }

    /// @see RoomSessionPetCommandsUpdateEvent.as::get petId
    public int petId { get; }

    /// @see RoomSessionPetCommandsUpdateEvent.as::get allCommands (AS3 Array → IReadOnlyList<int>)
    public IReadOnlyList<int> allCommands { get; }

    /// @see RoomSessionPetCommandsUpdateEvent.as::get enabledCommands (AS3 Array → IReadOnlyList<int>)
    public IReadOnlyList<int> enabledCommands { get; }
}
