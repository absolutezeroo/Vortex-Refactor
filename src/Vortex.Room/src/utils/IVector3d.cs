namespace Vortex.Room.Utils;

/// <summary>
/// Read-only 3D vector interface for room coordinate system.
/// </summary>
/// @see com.sulake.room.utils.IVector3d
public interface IVector3d
{
    double X { get; }

    double Y { get; }

    double Z { get; }

    double Length { get; }
}
