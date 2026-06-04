// @see com.sulake.habbo.communication.messages.parser.room.pets.PetLevelUpdateMessageParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

/// @see com.sulake.habbo.communication.messages.parser.room.pets.PetLevelUpdateMessageParser
public class PetLevelUpdateMessageEventParser : IMessageParser
{
    public int RoomIndex { get; private set; }
    public int PetId { get; private set; }
    public int Level { get; private set; }

    public bool Flush()
    {
        RoomIndex = 0;
        PetId = 0;
        Level = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        RoomIndex = param1.ReadInteger();
        PetId = param1.ReadInteger();
        Level = param1.ReadInteger();
        return true;
    }
}
