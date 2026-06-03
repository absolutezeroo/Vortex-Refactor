using Vortex.Room.Utils;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.class_1761
public class WiredUserMoveData(
    int userIndex, Vector3d source, Vector3d target,
    string moveType, double animationTime, double bodyDirection, double headDirection)
{
    public int UserIndex => userIndex;
    public Vector3d Source => source;
    public Vector3d Target => target;
    public string MoveType => moveType;
    public double AnimationTime => animationTime;
    public double BodyDirection => bodyDirection;
    public double HeadDirection => headDirection;
}
