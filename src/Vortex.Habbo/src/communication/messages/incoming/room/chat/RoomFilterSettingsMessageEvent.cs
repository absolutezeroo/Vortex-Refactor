using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Chat;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Chat;

/// @see com.sulake.habbo.communication.messages.incoming.room.chat.RoomFilterSettingsMessageEvent
public class RoomFilterSettingsMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(RoomFilterSettingsMessageEventParser));
