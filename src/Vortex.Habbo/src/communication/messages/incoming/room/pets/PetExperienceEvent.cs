using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Pets;

/// @see com.sulake.habbo.communication.messages.incoming.room.pets.PetExperienceEvent
public class PetExperienceEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(PetExperienceMessageEventParser))
{
    public int petId => ((PetExperienceMessageEventParser)parser!).petId;
    public int petRoomIndex => ((PetExperienceMessageEventParser)parser!).petRoomIndex;
    public int gainedExperience => ((PetExperienceMessageEventParser)parser!).gainedExperience;
}
