using Vortex.Room.Utils;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.class_1690
public class WiredFurniMoveData(
    int furniId, Vector3d source, Vector3d target, double animationTime, double rotation)
{
    public int FurniId => furniId;
    public Vector3d Source => source;
    public Vector3d Target => target;
    public double AnimationTime => animationTime;
    public double Rotation => rotation;
}
