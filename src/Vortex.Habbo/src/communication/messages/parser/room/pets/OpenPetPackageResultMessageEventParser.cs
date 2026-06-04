// @see com.sulake.habbo.communication.messages.parser.room.pets.OpenPetPackageResultMessageParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

/// @see com.sulake.habbo.communication.messages.parser.room.pets.OpenPetPackageResultMessageParser
public class OpenPetPackageResultMessageEventParser : IMessageParser
{
    public int ObjectId { get; private set; }
    public int NameValidationStatus { get; private set; }
    public string? NameValidationInfo { get; private set; }

    public bool Flush()
    {
        ObjectId = 0;
        NameValidationStatus = 0;
        NameValidationInfo = null;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        ObjectId = param1.ReadInteger();
        NameValidationStatus = param1.ReadInteger();
        NameValidationInfo = param1.ReadString();
        return true;
    }
}
