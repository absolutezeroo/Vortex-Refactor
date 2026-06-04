using System;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Pets;

/// @see com.sulake.habbo.communication.messages.incoming.room.pets.PetCommandsMessageEvent
public class PetCommandsMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(PetCommandsMessageEventParser));
