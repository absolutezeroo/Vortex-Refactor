namespace Vortex.Habbo.Room.Object;

/// <summary>
/// Edge mask segment data for plane occlusion cutouts along plane sides.
/// </summary>
/// @see com.sulake.habbo.room.object.RoomPlaneMaskData
public class RoomPlaneMaskData(double leftSideLoc, double rightSideLoc, double leftSideLength, double rightSideLength)
{
    public double LeftSideLoc => leftSideLoc;

    public double RightSideLoc => rightSideLoc;

    public double LeftSideLength => leftSideLength;

    public double RightSideLength => rightSideLength;
}
