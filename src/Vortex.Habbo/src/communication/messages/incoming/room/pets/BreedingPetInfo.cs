// @see com.sulake.habbo.communication.messages.incoming.room.pets.BreedingPetInfo

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Pets;

/// @see com.sulake.habbo.communication.messages.incoming.room.pets.BreedingPetInfo
public class BreedingPetInfo
{
    /// @see BreedingPetInfo.as::BreedingPetInfo
    public BreedingPetInfo(IMessageDataWrapper wrapper)
    {
        webId = wrapper.ReadInteger();
        name = wrapper.ReadString();
        level = wrapper.ReadInteger();
        figure = wrapper.ReadString();
        owner = wrapper.ReadString();
    }

    /// @see BreedingPetInfo.as::get webId
    public int webId { get; }

    /// @see BreedingPetInfo.as::get name
    public string name { get; }

    /// @see BreedingPetInfo.as::get level
    public int level { get; }

    /// @see BreedingPetInfo.as::get figure
    public string figure { get; }

    /// @see BreedingPetInfo.as::get owner
    public string owner { get; }
}
