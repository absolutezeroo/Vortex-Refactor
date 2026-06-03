using Vortex.Core.Communication.Messages;
using Vortex.Room.Object;

namespace Vortex.Habbo.Room.Object.Data;

/// <summary>
/// Key-value map StuffData (format key 1). Stores furniture state as a
/// dictionary of string key-value pairs.
/// </summary>
/// @see com.sulake.habbo.room.object.data.MapStuffData
public class MapStuffData(Dictionary<string, string>? data = null) : StuffDataBase, IStuffData
{
    public const int FORMAT_KEY = 1;

    private const string STATE_DEFAULT_KEY = "state";
    private const string RARITY_KEY = "rarity";

    private Dictionary<string, string> _data = data ?? new Dictionary<string, string>();

    public override int RarityLevel
    {
        get
        {
            if (_data.TryGetValue(RARITY_KEY, out string? rarity) && int.TryParse(rarity, out int level))
            {
                return level;
            }
            return -1;
        }
    }

    public override void InitializeFromIncomingMessage(IMessageDataWrapper wrapper)
    {
        _data = new Dictionary<string, string>();
        int count = wrapper.ReadInteger();
        for (int i = 0;
             i < count;
             i++)
        {
            string key = wrapper.ReadString();
            string value = wrapper.ReadString();
            _data[key] = value;
        }
        base.InitializeFromIncomingMessage(wrapper);
    }

    public override void InitializeFromRoomObjectModel(IRoomObjectModel model)
    {
        base.InitializeFromRoomObjectModel(model);
        _data = model.GetStringToStringMap(RoomObjectVariableEnum.FURNITURE_DATA) ?? new Dictionary<string, string>();
    }

    public override void WriteRoomObjectModel(IRoomObjectModelController model)
    {
        base.WriteRoomObjectModel(model);
        model.SetNumber(RoomObjectVariableEnum.FURNITURE_DATA_FORMAT, FORMAT_KEY);
        model.SetStringToStringMap(RoomObjectVariableEnum.FURNITURE_DATA, _data);
    }

    public override string GetLegacyString()
    {
        return _data.GetValueOrDefault(STATE_DEFAULT_KEY, "");
    }

    public override bool Compare(IStuffData other)
    {
        return false;
    }

    public string? GetValue(string key)
    {
        return _data.GetValueOrDefault(key);
    }
}
