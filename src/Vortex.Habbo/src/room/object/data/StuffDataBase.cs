using System.Globalization;
using System.Text.Json;

using Vortex.Core.Communication.Messages;
using Vortex.Room.Object;

namespace Vortex.Habbo.Room.Object.Data;

/// <summary>
/// Abstract base class for all StuffData types. Handles unique serial number
/// and edition size reading from the wire protocol when the UNIQUE_SERIAL_NUMBER
/// flag is set.
/// </summary>
/// @see com.sulake.habbo.room.object.data.StuffDataBase
public abstract class StuffDataBase : IStuffData
{
    private int _flags;

    public int Flags
    {
        set => _flags = value;
    }

    public int UniqueSerialNumber { get; set; }

    public int UniqueSeriesSize { get; set; }

    public virtual int RarityLevel => -1;

    public int State
    {
        get
        {
            if (double.TryParse(GetLegacyString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
            {
                return (int)val;
            }
            return -1;
        }
    }

    public virtual void InitializeFromIncomingMessage(IMessageDataWrapper wrapper)
    {
        if ((_flags & StuffDataFlagsEnum.UNIQUE_SERIAL_NUMBER) > 0)
        {
            UniqueSerialNumber = wrapper.ReadInteger();
            UniqueSeriesSize = wrapper.ReadInteger();
        }
    }

    public virtual void InitializeFromRoomObjectModel(IRoomObjectModel model)
    {
        UniqueSerialNumber = (int)model.GetNumber(RoomObjectVariableEnum.FURNITURE_UNIQUE_SERIAL_NUMBER);
        UniqueSeriesSize = (int)model.GetNumber(RoomObjectVariableEnum.FURNITURE_UNIQUE_EDITION_SIZE);
    }

    public virtual void WriteRoomObjectModel(IRoomObjectModelController model)
    {
        model.SetNumber(RoomObjectVariableEnum.FURNITURE_UNIQUE_SERIAL_NUMBER, UniqueSerialNumber);
        model.SetNumber(RoomObjectVariableEnum.FURNITURE_UNIQUE_EDITION_SIZE, UniqueSeriesSize);
    }

    public virtual string GetLegacyString()
    {
        return "";
    }

    public virtual bool Compare(IStuffData other)
    {
        return false;
    }

    public virtual string GetJSONValue(string key)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(GetLegacyString());
            if (doc.RootElement.TryGetProperty(key, out JsonElement prop))
            {
                return prop.ToString();
            }
        }
        catch
        {
            // Ignore parse errors
        }
        return "";
    }
}
