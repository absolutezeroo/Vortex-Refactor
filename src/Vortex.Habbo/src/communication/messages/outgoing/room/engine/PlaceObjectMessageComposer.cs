using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Engine;

/// @see com.sulake.habbo.communication.messages.outgoing.room.engine.PlaceObjectMessageComposer
public class PlaceObjectMessageComposer(int objectId, int category, string wallLocation, int x = 0, int y = 0, int direction = 0)
    : IMessageComposer
{
    public void Dispose() { }

    public List<object> GetMessageArray()
    {
        return (category - 10) switch
        {
            0 => [$"{objectId} {x} {y} {direction}"],
            10 => [$"{objectId} {wallLocation}"],
            _ => [],
        };
    }
}
