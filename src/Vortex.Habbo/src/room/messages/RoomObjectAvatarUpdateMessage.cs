using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar movement update with head direction, stand-up capability, and base Y offset.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarUpdateMessage
public class RoomObjectAvatarUpdateMessage
(
    IVector3d? location,
    IVector3d? targetLocation,
    IVector3d? direction,
    int dirHead,
    bool canStandUp,
    double baseY,
    double animationTime = double.NaN,
    bool isSlideUpdate = false
)
    : RoomObjectMoveUpdateMessage(location, targetLocation, direction, animationTime, isSlideUpdate)
{
    public int DirHead => dirHead;
    public bool CanStandUp => canStandUp;
    public double BaseY => baseY;
}
