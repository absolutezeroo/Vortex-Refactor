using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Furniture;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Furniture;

/// @see com.sulake.habbo.communication.messages.incoming.room.furniture.RoomDimmerPresetsMessageEvent
public class RoomDimmerPresetsMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(RoomDimmerPresetsMessageEventParser));
