using Godot;

namespace Vortex.Room.Utils;

/// <summary>
/// Interface for isometric room coordinate projection.
/// </summary>
/// @see com.sulake.room.utils.IRoomGeometry
public interface IRoomGeometry
{
    double Scale { get; }

    IVector3d DirectionAxis { get; }

    IVector3d Direction { get; }

    int UpdateId { get; }

    double ZScale { set; }

    IVector3d? GetCoordinatePosition(IVector3d position);

    Vector2? GetScreenPoint(IVector3d position);

    IVector3d? GetScreenPosition(IVector3d position);

    Vector2? GetPlanePosition(Vector2 screenPoint, IVector3d planeOrigin, IVector3d planeXAxis, IVector3d planeYAxis);

    void SetDisplacement(IVector3d position, IVector3d displacement);

    void AdjustLocation(IVector3d location, double amount);

    void PerformZoom();

    void PerformZoomOut();

    void PerformZoomIn();

    bool IsZoomedIn();
}
