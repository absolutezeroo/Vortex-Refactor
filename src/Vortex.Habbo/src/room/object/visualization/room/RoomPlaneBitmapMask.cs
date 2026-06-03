namespace Vortex.Habbo.Room.Object.Visualization.Room;

/// @see com.sulake.habbo.room.object.visualization.room.RoomPlaneBitmapMask
public class RoomPlaneBitmapMask(string type, double leftSideLoc, double rightSideLoc)
{
    public string Type { get; } = type;

    public double LeftSideLoc { get; } = leftSideLoc;

    public double RightSideLoc { get; } = rightSideLoc;
}
