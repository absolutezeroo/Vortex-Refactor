using Vortex.Room.Messages;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Update message for object movement with target location, animation timing,
/// and slide/skip flags.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectMoveUpdateMessage
public class RoomObjectMoveUpdateMessage
(
    IVector3d? location,
    IVector3d? targetLocation,
    IVector3d? direction,
    double animationTime = double.NaN,
    bool isSlideUpdate = false,
    bool skipPositionUpdate = false
)
    : RoomObjectUpdateMessage(location, direction)
{
    public bool IsSlideUpdate => isSlideUpdate;
    public double AnimationTime => animationTime;
    public bool SkipPositionUpdate => skipPositionUpdate;
    public IVector3d? TargetLoc => targetLocation ?? Location;
    public IVector3d? RealTargetLoc => targetLocation;
}
