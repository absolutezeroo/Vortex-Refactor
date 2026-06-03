using System;

using Godot;

using Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Animated;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.animated.class_3787
public class LandscapePlane : Basic.Plane
{
    public const uint DEFAULT_COLOR = 0xFFFFFF;
    public const double HORIZONTAL_ANGLE_DEFAULT = 45;
    public const double VERTICAL_ANGLE_DEFAULT = 30;

    private int _width;
    private int _height;

    public override bool IsStatic(int scale)
    {
        PlaneVisualization? viz = GetPlaneVisualization(scale);

        if (viz != null)
        {
            return !viz.HasAnimationLayers;
        }

        return base.IsStatic(scale);
    }

    public void InitializeDimensions(int width, int height)
    {
        if (width < 0)
        {
            width = 0;
        }

        if (height < 0)
        {
            height = 0;
        }

        if (width != _width || height != _height)
        {
            _width = width;
            _height = height;
        }
    }

    public Image? Render(
        Image? canvas, double width, double height, double scale,
        IVector3d normal, bool useTexture,
        double offsetX, double offsetY,
        double topAlignOffset, double bottomAlignOffset,
        int timeStamp)
    {
        PlaneVisualization? viz = GetPlaneVisualization((int)scale);

        if (viz?.Geometry == null)
        {
            return null;
        }

        Vector2? origin = viz.Geometry.GetScreenPoint(new Vector3d(0, 0, 0));
        Vector2? topPoint = viz.Geometry.GetScreenPoint(new Vector3d(0, 0, 1));
        Vector2? rightPoint = viz.Geometry.GetScreenPoint(new Vector3d(0, 1, 0));

        if (origin == null || topPoint == null || rightPoint == null)
        {
            return null;
        }

        double screenWidth = Math.Round(Math.Abs(origin.Value.X - rightPoint.Value.X) * width / viz.Geometry.Scale);
        double screenHeight = Math.Round(Math.Abs(origin.Value.Y - topPoint.Value.Y) * height / viz.Geometry.Scale);

        int animOffsetX = (int)(offsetX * Math.Abs(origin.Value.X - rightPoint.Value.X));
        int animOffsetY = (int)(offsetY * Math.Abs(origin.Value.Y - topPoint.Value.Y));
        int animTileWidth = (int)(topAlignOffset * Math.Abs(origin.Value.X - rightPoint.Value.X));
        int animTileHeight = (int)(bottomAlignOffset * Math.Abs(origin.Value.Y - topPoint.Value.Y));

        return viz.Render(
            canvas, (int)screenWidth, (int)screenHeight,
            normal, useTexture,
            animOffsetX, animOffsetY,
            animTileWidth, animTileHeight,
            topAlignOffset, bottomAlignOffset,
            timeStamp);
    }
}
