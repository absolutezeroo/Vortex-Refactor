using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Action;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Action;

/// @see com.sulake.habbo.communication.messages.incoming.room.action.DanceMessageEvent
public class DanceMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(DanceMessageEventParser));
