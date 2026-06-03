using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Layout;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Layout;

/// @see com.sulake.habbo.communication.messages.incoming.room.layout.RoomEntryTileMessageEvent
public class RoomEntryTileMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(RoomEntryTileMessageEventParser));
