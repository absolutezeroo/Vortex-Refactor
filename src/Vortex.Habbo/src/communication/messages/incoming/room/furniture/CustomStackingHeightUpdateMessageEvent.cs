using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Furniture;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Furniture;

/// @see com.sulake.habbo.communication.messages.incoming.room.furniture.CustomStackingHeightUpdateMessageEvent
public class CustomStackingHeightUpdateMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(CustomStackingHeightUpdateMessageEventParser));
