using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Furniture;

/// @see com.sulake.habbo.communication.messages.outgoing.room.furniture.RoomDimmerSavePresetMessageComposer
public class RoomDimmerSavePresetMessageComposer(int presetId, int type, uint color, int brightness, bool apply) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        // TODO(as3-port): verify payload order from AS3 source
        return [presetId, type, (int)color, brightness, apply];
    }
}
