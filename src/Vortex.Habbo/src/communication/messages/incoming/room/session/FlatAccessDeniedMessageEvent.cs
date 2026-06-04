using System;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Session;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Session;

/// @see com.sulake.habbo.communication.messages.incoming.room.session.FlatAccessDeniedMessageEvent
public class FlatAccessDeniedMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(FlatAccessDeniedMessageEventParser));
