using Vortex.Core.Communication.Messages;
using Vortex.Room.Object;

namespace Vortex.Habbo.Room.Object.Data;

/// <summary>
/// StuffData type with no additional data payload (format key 4).
/// </summary>
/// @see com.sulake.habbo.room.object.data.EmptyStuffData
public class EmptyStuffData : StuffDataBase, IStuffData
{
    public override void InitializeFromIncomingMessage(IMessageDataWrapper wrapper)
    {
        base.InitializeFromIncomingMessage(wrapper);
    }

    public override void InitializeFromRoomObjectModel(IRoomObjectModel model)
    {
        base.InitializeFromRoomObjectModel(model);
    }

    public override void WriteRoomObjectModel(IRoomObjectModelController model)
    {
        base.WriteRoomObjectModel(model);
    }

    public override string GetLegacyString()
    {
        return "";
    }

    public override bool Compare(IStuffData other)
    {
        return base.Compare(other);
    }
}
