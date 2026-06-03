using Vortex.Core.Communication.Messages;
using Vortex.Room.Object;

namespace Vortex.Habbo.Room.Object.Data;

/// <summary>
/// Vote result StuffData (format key 3). Stores a state string and an
/// integer vote result count.
/// </summary>
/// @see com.sulake.habbo.room.object.data.VoteResultStuffData
public class VoteResultStuffData : StuffDataBase, IStuffData
{
    public const int FORMAT_KEY = 3;

    private const string INTERNAL_STATE_KEY = "s";
    private const string INTERNAL_RESULT_KEY = "r";

    private string _state = "";
    private int _result;

    public int Result => _result;

    public override void InitializeFromIncomingMessage(IMessageDataWrapper wrapper)
    {
        _state = wrapper.ReadString();
        _result = wrapper.ReadInteger();
        base.InitializeFromIncomingMessage(wrapper);
    }

    public override void WriteRoomObjectModel(IRoomObjectModelController model)
    {
        base.WriteRoomObjectModel(model);
        model.SetNumber(RoomObjectVariableEnum.FURNITURE_DATA_FORMAT, FORMAT_KEY);
        Dictionary<string, string> map = new()
        {
            {
                INTERNAL_STATE_KEY, _state
            },
            {
                INTERNAL_RESULT_KEY, _result.ToString()
            },
        };
        model.SetStringToStringMap(RoomObjectVariableEnum.FURNITURE_DATA, map);
    }

    public override void InitializeFromRoomObjectModel(IRoomObjectModel model)
    {
        base.InitializeFromRoomObjectModel(model);
        Dictionary<string, string>? map = model.GetStringToStringMap(RoomObjectVariableEnum.FURNITURE_DATA);
        if (map != null)
        {
            map.TryGetValue(INTERNAL_STATE_KEY, out string? state);
            _state = state ?? "";
            if (map.TryGetValue(INTERNAL_RESULT_KEY, out string? result))
            {
                int.TryParse(result, out _result);
            }
        }
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
