// @see com.sulake.habbo.communication.messages.parser.room.pets.ConfirmBreedingRequestMessageParser

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Pets;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

/// @see com.sulake.habbo.communication.messages.parser.room.pets.ConfirmBreedingRequestMessageParser
public class ConfirmBreedingRequestMessageEventParser : IMessageParser
{
    public int NestId { get; private set; }
    public BreedingPetInfo? Pet1 { get; private set; }
    public BreedingPetInfo? Pet2 { get; private set; }
    public IReadOnlyList<int> RarityCategories { get; private set; } = [];
    public int ResultPetTypeId { get; private set; }

    public bool Flush()
    {
        NestId = 0;
        Pet1 = null;
        Pet2 = null;
        RarityCategories = [];
        ResultPetTypeId = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        NestId = param1.ReadInteger();
        Pet1 = new BreedingPetInfo(param1);
        Pet2 = new BreedingPetInfo(param1);
        int count = param1.ReadInteger();
        List<int> cats = new List<int>(count);
        for (int i = 0; i < count; i++)
        {
            cats.Add(param1.ReadInteger());
        }

        RarityCategories = cats;
        ResultPetTypeId = param1.ReadInteger();
        return true;
    }
}
