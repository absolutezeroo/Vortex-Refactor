namespace Vortex.Habbo.Room.Object.Logic;

using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Messages;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureYoutubeLogic (class_3512)
public class FurnitureYoutubeLogic : FurnitureLogic
{
    public override string? Widget => "RWE_YOUTUBE";

    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectDataRequestEvent.URL_PREFIX];
        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is not RoomObjectDataUpdateMessage || Object == null)
        {
            return;
        }

        string? urlPrefix = Object.ModelController.GetString("furniture_youtube_url_prefix");

        if (string.IsNullOrEmpty(urlPrefix))
        {
            DispatchEvent(new RoomObjectDataRequestEvent(
                RoomObjectDataRequestEvent.URL_PREFIX, Object));
        }
    }
}
