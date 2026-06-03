using Vortex.Core.Communication.Messages;
using Vortex.Room.Object;

namespace Vortex.Habbo.Room;

/// <summary>
/// Interface for furniture state data that can be serialized to/from the wire protocol
/// and room object models.
/// </summary>
/// @see com.sulake.habbo.room.IStuffData
public interface IStuffData
{
    int Flags { set; }
    int UniqueSerialNumber { get; set; }
    int UniqueSeriesSize { get; set; }
    int RarityLevel { get; }

    void InitializeFromIncomingMessage(IMessageDataWrapper wrapper);
    void InitializeFromRoomObjectModel(IRoomObjectModel model);
    void WriteRoomObjectModel(IRoomObjectModelController model);
    string GetLegacyString();
    string GetJSONValue(string key);
    bool Compare(IStuffData other);
}
