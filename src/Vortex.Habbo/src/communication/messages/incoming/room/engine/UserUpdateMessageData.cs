namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;

/// @see com.sulake.habbo.communication.messages.incoming.room.engine.class_1753
public class UserUpdateMessageData(
    int id,
    double x,
    double y,
    double z,
    double localZ,
    int dirHead,
    int dir,
    double targetX,
    double targetY,
    double targetZ,
    bool isMoving,
    bool canStandUp,
    List<AvatarActionMessageData> actions,
    bool skipPositionUpdate)
{
    public int Id => id;
    public double X => x;
    public double Y => y;
    public double Z => z;
    public double LocalZ => localZ;
    public double TargetX => targetX;
    public double TargetY => targetY;
    public double TargetZ => targetZ;
    public int Dir => dir;
    public int DirHead => dirHead;
    public bool IsMoving => isMoving;
    public bool CanStandUp => canStandUp;
    public bool SkipPositionUpdate => skipPositionUpdate;
    public List<AvatarActionMessageData> Actions => [.. actions];
}
