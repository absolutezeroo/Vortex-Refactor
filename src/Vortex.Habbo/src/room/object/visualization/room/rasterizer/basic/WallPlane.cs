using System;

using Godot;

using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.basic.WallPlane
public class WallPlane : Plane
{
    public const uint DEFAULT_COLOR = 0xFFFFFF;
    public const double HORIZONTAL_ANGLE_DEFAULT = 45;
    public const double VERTICAL_ANGLE_DEFAULT = 30;

    public Image? Render(
        Image? canvas, double width, double height, double scale,
        IVector3d normal, bool useTexture)
    {
        PlaneVisualization? viz = GetPlaneVisualization((int)scale);

        if (viz?.Geometry == null)
        {
            return null;
        }

        Vector2? origin = viz.Geometry.GetScreenPoint(new Vector3d(0, 0, 0));
        Vector2? topPoint = viz.Geometry.GetScreenPoint(new Vector3d(0, 0, height / viz.Geometry.Scale));
        Vector2? rightPoint = viz.Geometry.GetScreenPoint(new Vector3d(0, width / viz.Geometry.Scale, 0));

        if (origin == null || topPoint == null || rightPoint == null)
        {
            return viz.Render(canvas, (int)width, (int)height, normal, useTexture);
        }

        width = Math.Round(Math.Abs(origin.Value.X - rightPoint.Value.X));
        height = Math.Round(Math.Abs(origin.Value.Y - topPoint.Value.Y));

        return viz.Render(canvas, (int)width, (int)height, normal, useTexture);
    }
}
