using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

/// @see com.sulake.habbo.communication.messages.parser.inventory.pets.PetFigureData
public class PetFigureData
{
    private readonly List<int> _customParts = [];

    public PetFigureData(IMessageDataWrapper param1)
    {
        typeId = param1.ReadInteger();
        paletteId = param1.ReadInteger();
        color = param1.ReadString();
        breedId = param1.ReadInteger();
        int count = param1.ReadInteger();

        for (int i = 0; i < count; i++)
        {
            _customParts.Add(param1.ReadInteger());
            _customParts.Add(param1.ReadInteger());
            _customParts.Add(param1.ReadInteger());
        }
    }

    public int typeId { get; }
    public int paletteId { get; }
    public string color { get; }
    public int breedId { get; }
    public IReadOnlyList<int> customParts => _customParts;
    public int customPartCount => _customParts.Count / 3;

    /// @see com.sulake.habbo.communication.messages.parser.inventory.pets.PetFigureData::get figureString
    public string figureString
    {
        get
        {
            string s = $"{typeId} {paletteId} {color} {customPartCount}";
            foreach (int part in _customParts)
            {
                s += $" {part}";
            }
            return s;
        }
    }
}
