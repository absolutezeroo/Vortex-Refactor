namespace Vortex.Habbo.Room.Object;

/// <summary>
/// Rectangular floor hole definition for plane cutouts in room geometry.
/// </summary>
/// @see com.sulake.habbo.room.object.RoomFloorHole
public class RoomFloorHole(int x, int y, int width, int height)
{
    public int X => x;
    public int Y => y;
    public int Width => width;
    public int Height => height;
}
