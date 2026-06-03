namespace Vortex.Habbo.Room.Object.Visualization.Room;

/// @see com.sulake.habbo.room.object.visualization.room.RoomPlaneRectangleMask
public class RoomPlaneRectangleMask(double leftSideLoc, double rightSideLoc,
    double leftSideLength, double rightSideLength)
{
    public double LeftSideLoc { get; } = leftSideLoc;

    public double RightSideLoc { get; } = rightSideLoc;

    public double LeftSideLength { get; } = leftSideLength;

    public double RightSideLength { get; } = rightSideLength;
}
