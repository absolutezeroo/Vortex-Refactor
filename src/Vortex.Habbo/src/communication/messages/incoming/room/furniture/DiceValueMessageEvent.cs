using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Furniture;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Furniture;

/// @see com.sulake.habbo.communication.messages.incoming.room.furniture.DiceValueMessageEvent
public class DiceValueMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(DiceValueMessageEventParser));
