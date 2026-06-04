using System;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Poll;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Poll;

/// @see com.sulake.habbo.communication.messages.incoming.room.poll.PollContentsEvent
public class PollContentsEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(PollContentsMessageEventParser));
