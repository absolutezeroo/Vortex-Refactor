using Vortex.Core.Communication.Messages;
using Vortex.Room.Object;

namespace Vortex.Habbo.Room.Object.Data;

/// <summary>
/// String array StuffData (format key 2). Stores furniture state as an
/// ordered list of strings where index 0 is the legacy state string.
/// </summary>
/// @see com.sulake.habbo.room.object.data.StringArrayStuffData
public class StringArrayStuffData : StuffDataBase, IStuffData
{
    public const int FORMAT_KEY = 2;

    private const int STATE_DEFAULT_INDEX = 0;

    private List<string> _data = [];

    public override void InitializeFromIncomingMessage(IMessageDataWrapper wrapper)
    {
        _data = [];
        int count = wrapper.ReadInteger();
        for (int i = 0; i < count; i++)
        {
            _data.Add(wrapper.ReadString());
        }
        base.InitializeFromIncomingMessage(wrapper);
    }

    public override void InitializeFromRoomObjectModel(IRoomObjectModel model)
    {
        base.InitializeFromRoomObjectModel(model);
        string[]? arr = model.GetStringArray(RoomObjectVariableEnum.FURNITURE_DATA);
        _data = arr != null ? new List<string>(arr) : [];
    }

    public override void WriteRoomObjectModel(IRoomObjectModelController model)
    {
        base.WriteRoomObjectModel(model);
        model.SetNumber(RoomObjectVariableEnum.FURNITURE_DATA_FORMAT, FORMAT_KEY);
        model.SetStringArray(RoomObjectVariableEnum.FURNITURE_DATA, _data.ToArray());
    }

    public override string GetLegacyString()
    {
        if (_data.Count == 0)
        {
            return "";
        }
        return _data[STATE_DEFAULT_INDEX];
    }

    public override bool Compare(IStuffData other)
    {
        if (other is not StringArrayStuffData otherArray)
        {
            return false;
        }
        for (int i = 0; i < _data.Count; i++)
        {
            if (i != STATE_DEFAULT_INDEX)
            {
                if (_data[i] != otherArray.GetValue(i))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public string GetValue(int index)
    {
        if (index >= 0 && index < _data.Count)
        {
            return _data[index];
        }
        return "";
    }

    public void SetArray(List<string> data)
    {
        _data = data;
    }
}
