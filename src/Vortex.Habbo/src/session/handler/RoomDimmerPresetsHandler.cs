// @see com.sulake.habbo.session.handler.RoomDimmerPresetsHandler

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Furniture;
using Vortex.Habbo.Communication.Messages.Parser.Room.Furniture;
using Vortex.Habbo.Session.Events;

namespace Vortex.Habbo.Session.Handler;

/// @see com.sulake.habbo.session.handler.RoomDimmerPresetsHandler
public class RoomDimmerPresetsHandler : BaseHandler
{
    /// @see RoomDimmerPresetsHandler.as::RoomDimmerPresetsHandler
    public RoomDimmerPresetsHandler(IConnection? connection, IRoomHandlerListener listener)
        : base(connection, listener)
    {
        if (connection == null)
        {
            return;
        }

        connection.AddMessageEvent(new RoomDimmerPresetsMessageEvent(OnRoomDimmerPresets));
    }

    /// @see RoomDimmerPresetsHandler.as::onRoomDimmerPresets
    private void OnRoomDimmerPresets(IMessageEvent ev)
    {
        RoomDimmerPresetsMessageEvent? dimmerEv = ev as RoomDimmerPresetsMessageEvent;
        if (dimmerEv == null)
        {
            return;
        }

        RoomDimmerPresetsMessageEventParser? parser = dimmerEv.parser as RoomDimmerPresetsMessageEventParser;
        if (parser == null)
        {
            return;
        }

        IRoomSession? session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        RoomSessionDimmerPresetsEvent presetsEvent = new RoomSessionDimmerPresetsEvent(RoomSessionDimmerPresetsEvent.ROOM_DIMMER_PRESETS, session);
        presetsEvent.selectedPresetId = parser.SelectedPresetId;
        for (int i = 0; i < parser.PresetCount; i++)
        {
            RoomDimmerPresetsMessageData? preset = parser.GetPreset(i);
            if (preset != null)
            {
                presetsEvent.StorePreset(preset.Id, preset.Type, preset.Color, preset.Light);
            }
        }
        listener?.events?.DispatchEvent(presetsEvent);
    }
}
