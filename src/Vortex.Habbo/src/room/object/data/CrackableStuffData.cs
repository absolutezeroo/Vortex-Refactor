using Vortex.Core.Communication.Messages;
using Vortex.Room.Object;

namespace Vortex.Habbo.Room.Object.Data;

/// <summary>
/// Crackable StuffData (format key 7). Stores a state string, current hit
/// count, and target hit count for breakable furniture items.
/// </summary>
/// @see com.sulake.habbo.room.object.data.CrackableStuffData
public class CrackableStuffData : StuffDataBase, IStuffData
{
    public const int FORMAT_KEY = 7;

    private const string INTERNAL_STATE_KEY = "furniture_crackable_state";
    private const string INTERNAL_HIT_KEY = "furniture_crackable_hits";
    private const string INTERNAL_TARGET_KEY = "furniture_crackable_target";

    private string _state = "";

    public int Hits { get; private set; }

    public int Target { get; private set; }

    public override void InitializeFromIncomingMessage(IMessageDataWrapper wrapper)
    {
        _state = wrapper.ReadString();
        Hits = wrapper.ReadInteger();
        Target = wrapper.ReadInteger();
        base.InitializeFromIncomingMessage(wrapper);
    }

    public override void WriteRoomObjectModel(IRoomObjectModelController model)
    {
        base.WriteRoomObjectModel(model);
        model.SetNumber("furniture_data_format", FORMAT_KEY);
        model.SetString(INTERNAL_STATE_KEY, _state);
        model.SetNumber(INTERNAL_HIT_KEY, Hits);
        model.SetNumber(INTERNAL_TARGET_KEY, Target);
    }

    public override void InitializeFromRoomObjectModel(IRoomObjectModel model)
    {
        base.InitializeFromRoomObjectModel(model);
        _state = model.GetString(INTERNAL_STATE_KEY) ?? "";
        Hits = (int)model.GetNumber(INTERNAL_HIT_KEY);
        Target = (int)model.GetNumber(INTERNAL_TARGET_KEY);
    }

    public override string GetLegacyString()
    {
        return _state;
    }

    public override bool Compare(IStuffData other)
    {
        return true;
    }

    public void SetString(string value)
    {
        _state = value;
    }
}
