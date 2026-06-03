using System;

using Godot;

using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.basic.FloorPlane
public class FloorPlane : Plane
{
    public const uint DEFAULT_COLOR = 0xFFFFFF;
    public const double HORIZONTAL_ANGLE_DEFAULT = 45;
    public const double VERTICAL_ANGLE_DEFAULT = 30;

    public Image? Render(
        Image? canvas, double width, double height, double scale,
        IVector3d normal, bool useTexture,
        double offsetX, double offsetY)
    {
        PlaneVisualization? viz = GetPlaneVisualization((int)scale);

        if (viz?.Geometry == null)
        {
            return null;
        }

        Vector2? origin = viz.Geometry.GetScreenPoint(new Vector3d(0, 0, 0));
        Vector2? rightPoint = viz.Geometry.GetScreenPoint(new Vector3d(0, height / viz.Geometry.Scale, 0));
        Vector2? bottomPoint = viz.Geometry.GetScreenPoint(new Vector3d(width / viz.Geometry.Scale, 0, 0));

        int intOffsetX = 0;
        int intOffsetY = 0;

        if (origin == null || rightPoint == null || bottomPoint == null)
        {
            return viz.Render(canvas, (int)width, (int)height, normal, useTexture, intOffsetX, intOffsetY);
        }

        width = Math.Round(Math.Abs(origin.Value.X - bottomPoint.Value.X));
        height = Math.Round(Math.Abs(origin.Value.X - rightPoint.Value.X));

        Vector2? unitPoint = viz.Geometry.GetScreenPoint(new Vector3d(1, 0, 0));
        double unitDist = origin.Value.X - unitPoint!.Value.X;
        intOffsetX = (int)(offsetX * Math.Abs(unitDist));
        intOffsetY = (int)(offsetY * Math.Abs(unitDist));

        return viz.Render(canvas, (int)width, (int)height, normal, useTexture, intOffsetX, intOffsetY);
    }
}
