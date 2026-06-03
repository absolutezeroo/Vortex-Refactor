using System.Globalization;

using Vortex.Core.Communication.Messages;
using Vortex.Room.Object;

namespace Vortex.Habbo.Room.Object.Data;

/// <summary>
/// Integer array StuffData (format key 5). Stores furniture state as an
/// ordered list of integers where index 0 is the legacy state value.
/// </summary>
/// @see com.sulake.habbo.room.object.data.IntArrayStuffData
public class IntArrayStuffData : StuffDataBase, IStuffData
{
    public const int FORMAT_KEY = 5;

    private const int STATE_DEFAULT_INDEX = 0;

    private List<int> _data = [];

    public override void InitializeFromIncomingMessage(IMessageDataWrapper wrapper)
    {
        _data = [];
        int count = wrapper.ReadInteger();
        for (int i = 0; i < count; i++)
        {
            _data.Add(wrapper.ReadInteger());
        }
        base.InitializeFromIncomingMessage(wrapper);
    }

    public override void InitializeFromRoomObjectModel(IRoomObjectModel model)
    {
        base.InitializeFromRoomObjectModel(model);
        _data = model.GetNumberArray(RoomObjectVariableEnum.FURNITURE_DATA) ?? [];
    }

    public override void WriteRoomObjectModel(IRoomObjectModelController model)
    {
        base.WriteRoomObjectModel(model);
        model.SetNumber(RoomObjectVariableEnum.FURNITURE_DATA_FORMAT, FORMAT_KEY);
        model.SetNumberArray(RoomObjectVariableEnum.FURNITURE_DATA, _data);
    }

    public override string GetLegacyString()
    {
        if (_data.Count == 0)
        {
            return "";
        }
        return _data[STATE_DEFAULT_INDEX].ToString(CultureInfo.InvariantCulture);
    }

    public override bool Compare(IStuffData other)
    {
        if (other is not IntArrayStuffData otherArray)
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

    public int GetValue(int index)
    {
        if (index >= 0 && index < _data.Count)
        {
            return _data[index];
        }
        return -1;
    }

    public void SetArray(List<int> data)
    {
        _data = data;
    }
}
