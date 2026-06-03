using System.Globalization;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;

/// @see com.sulake.habbo.communication.messages.incoming.room.engine.class_1766
public class ItemStateUpdateMessageData(int id, string itemData)
{
    public int Id => id;
    public string ItemData => itemData;
    public int State { get; } = double.TryParse(itemData, NumberStyles.Float, CultureInfo.InvariantCulture, out _)
        ? int.Parse(itemData, CultureInfo.InvariantCulture) : 0;
}
