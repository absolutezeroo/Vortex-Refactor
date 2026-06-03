using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Pets;

/// @see com.sulake.habbo.communication.messages.incoming.room.pets.PetFigureUpdateEvent
public class PetFigureUpdateEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(PetFigureUpdateMessageEventParser))
{
    public int roomIndex => ((PetFigureUpdateMessageEventParser)parser!).roomIndex;
    public int petId => ((PetFigureUpdateMessageEventParser)parser!).petId;
    public PetFigureData? figureData => ((PetFigureUpdateMessageEventParser)parser!).figureData;
    public bool hasSaddle => ((PetFigureUpdateMessageEventParser)parser!).hasSaddle;
    public bool isRiding => ((PetFigureUpdateMessageEventParser)parser!).isRiding;
}
