using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

/// @see com.sulake.habbo.communication.messages.parser.room.pets.PetFigureUpdateMessageParser
public class PetFigureUpdateMessageEventParser : IMessageParser
{
    public int roomIndex { get; private set; }
    public int petId { get; private set; }
    public PetFigureData? figureData { get; private set; }
    public bool hasSaddle { get; private set; }
    public bool isRiding { get; private set; }

    public bool Flush()
    {
        roomIndex = 0;
        petId = 0;
        figureData = null;
        hasSaddle = false;
        isRiding = false;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        roomIndex = param1.ReadInteger();
        petId = param1.ReadInteger();
        figureData = new PetFigureData(param1);
        hasSaddle = param1.ReadBoolean();
        isRiding = param1.ReadBoolean();
        return true;
    }
}
