// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/geometry/Node3D.as

namespace Vortex.Habbo.Avatar.Geometry;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/geometry/Node3D.as
public class AvatarNode3D
{
    private readonly bool _hasOffset;

    /// @see Node3D.as::Node3D
    public AvatarNode3D(double param1, double param2, double param3)
    {
        TransformedLocation = new AvatarVector3D();
        Location = new AvatarVector3D(param1, param2, param3);

        if (param1 != 0 || param2 != 0 || param3 != 0)
        {
            _hasOffset = true;
        }
    }

    /// @see Node3D.as::get location
    public AvatarVector3D Location { get; }

    /// @see Node3D.as::get transformedLocation
    public AvatarVector3D TransformedLocation { get; private set; }

    /// @see Node3D.as::applyTransform
    public void ApplyTransform(AvatarMatrix4x4 param1)
    {
        if (_hasOffset)
        {
            TransformedLocation = param1.VectorMultiplication(Location);
        }
    }
}
