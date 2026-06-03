using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;

/// @see com.sulake.habbo.communication.messages.incoming.room.engine.ItemDataUpdateMessageEvent
public class ItemDataUpdateMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(ItemDataUpdateMessageEventParser));
