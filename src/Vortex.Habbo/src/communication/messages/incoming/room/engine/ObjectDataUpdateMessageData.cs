using Vortex.Habbo.Room;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;

/// @see com.sulake.habbo.communication.messages.incoming.room.engine.class_1721
public class ObjectDataUpdateMessageData(int id, int state, IStuffData data)
{
    public int Id => id;
    public int State => state;
    public IStuffData Data => data;
}
