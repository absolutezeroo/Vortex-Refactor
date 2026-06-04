using System;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Poll;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Poll;

/// @see com.sulake.habbo.communication.messages.incoming.room.poll.PollOfferEvent
public class PollOfferEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(PollOfferMessageEventParser));
