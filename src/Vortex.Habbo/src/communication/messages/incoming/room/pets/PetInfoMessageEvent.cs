using System;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Pets;

/// @see com.sulake.habbo.communication.messages.incoming.room.pets.PetInfoMessageEvent
public class PetInfoMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(PetInfoMessageEventParser));
