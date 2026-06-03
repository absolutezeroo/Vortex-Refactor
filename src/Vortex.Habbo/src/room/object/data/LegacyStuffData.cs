using Vortex.Core.Communication.Messages;
using Vortex.Room.Object;

namespace Vortex.Habbo.Room.Object.Data;

/// <summary>
/// Simple string-based StuffData (format key 0). The legacy format stores
/// furniture state as a single string value.
/// </summary>
/// @see com.sulake.habbo.room.object.data.LegacyStuffData
public class LegacyStuffData : StuffDataBase, IStuffData
{
    public const int FORMAT_KEY = 0;

    private string _data = "";

    public override void InitializeFromIncomingMessage(IMessageDataWrapper wrapper)
    {
        _data = wrapper.ReadString();
        base.InitializeFromIncomingMessage(wrapper);
    }

    public override void InitializeFromRoomObjectModel(IRoomObjectModel model)
    {
        base.InitializeFromRoomObjectModel(model);
        _data = model.GetString(RoomObjectVariableEnum.FURNITURE_DATA) ?? "";
    }

    public override void WriteRoomObjectModel(IRoomObjectModelController model)
    {
        base.WriteRoomObjectModel(model);
        model.SetNumber(RoomObjectVariableEnum.FURNITURE_DATA_FORMAT, FORMAT_KEY);
        model.SetString(RoomObjectVariableEnum.FURNITURE_DATA, _data);
    }

    public override string GetLegacyString()
    {
        return _data;
    }

    public override bool Compare(IStuffData other)
    {
        return _data == other.GetLegacyString();
    }

    public void SetString(string value)
    {
        _data = value;
    }
}
