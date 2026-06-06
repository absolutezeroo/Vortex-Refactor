namespace Vortex.Habbo.Room.Object.Logic;

using System.Globalization;

using Messages;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureSongDiskLogic
public class FurnitureSongDiskLogic : FurnitureLogic
{
    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is not RoomObjectDataUpdateMessage || Object == null)
        {
            return;
        }

        double realRoom = Object.Model.GetNumber("furniture_real_room_object");

        if (realRoom != 1)
        {
            return;
        }

        string? extras = Object.ModelController.GetString("furniture_extras");

        if (extras != null && int.TryParse(extras, NumberStyles.Integer, CultureInfo.InvariantCulture, out int songId))
        {
            Object.ModelController.SetString("RWEIEP_INFOSTAND_EXTRA_PARAM", "RWEIEP_SONGDISK" + songId);
        }
    }
}
