namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.class_1655
public class WiredUserDirUpdateData(int userIndex, double bodyDirection, double headDirection)
{
    public int UserIndex => userIndex;
    public double BodyDirection => bodyDirection;
    public double HeadDirection => headDirection;
}
