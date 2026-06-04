using System;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Quiz;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Quiz;

/// @see com.sulake.habbo.communication.messages.incoming.room.quiz.QuestionEvent
public class QuestionEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(QuestionMessageEventParser));
