// @see com.sulake.habbo.communication.messages.parser.room.pets.OpenPetPackageRequestedMessageParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

/// @see com.sulake.habbo.communication.messages.parser.room.pets.OpenPetPackageRequestedMessageParser
public class OpenPetPackageRequestedMessageEventParser : IMessageParser
{
    public int ObjectId { get; private set; }
    public PetFigureData? FigureData { get; private set; }

    public bool Flush()
    {
        ObjectId = 0;
        FigureData = null;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        ObjectId = param1.ReadInteger();
        FigureData = new PetFigureData(param1);
        return true;
    }
}
