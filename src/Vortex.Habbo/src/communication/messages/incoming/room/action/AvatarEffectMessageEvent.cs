using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Action;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Action;

/// @see com.sulake.habbo.communication.messages.incoming.room.action.AvatarEffectMessageEvent
public class AvatarEffectMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(AvatarEffectMessageEventParser));
