using System;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Data;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Data;

/// @see com.sulake.habbo.communication.messages.incoming.room.data.GetGuestRoomResultEvent
public class GetGuestRoomResultEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(GetGuestRoomResultMessageEventParser));
