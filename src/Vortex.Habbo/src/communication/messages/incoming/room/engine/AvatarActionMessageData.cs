namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;

/// @see com.sulake.habbo.communication.messages.incoming.room.engine.class_1711
public class AvatarActionMessageData(string actionType, string actionParameter)
{
    public string ActionType => actionType;
    public string ActionParameter => actionParameter;

    public override string ToString()
    {
        return actionType + ":" + actionParameter;
    }
}
