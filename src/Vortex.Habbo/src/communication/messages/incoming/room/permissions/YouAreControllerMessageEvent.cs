using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Permissions;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Permissions;

/// @see com.sulake.habbo.communication.messages.incoming.room.permissions.YouAreControllerMessageEvent
public class YouAreControllerMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(YouAreControllerMessageEventParser));
