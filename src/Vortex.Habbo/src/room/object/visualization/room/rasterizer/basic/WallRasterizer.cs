using System.Xml.Linq;

using Godot;

using Vortex.Habbo.Room.Object.Visualization.Room.Utils;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.basic.WallRasterizer
public class WallRasterizer : PlaneRasterizer
{
    protected override void InitializePlanes()
    {
        if (Data == null)
        {
            return;
        }

        XElement? wallsElement = Data.Element("walls");

        if (wallsElement != null)
        {
            ParseWalls(wallsElement);
        }
    }

    public override PlaneBitmapData? Render(
        Image? canvas, string type,
        double width, double height, double scale,
        IVector3d normal, bool useTexture,
        double offsetX = 0, double offsetY = 0,
        double topAlignOffset = 0, double bottomAlignOffset = 0,
        int timeStamp = 0)
    {
        WallPlane? plane = GetPlane(type) as WallPlane ?? GetPlane("default") as WallPlane;

        if (plane == null)
        {
            return null;
        }

        if (canvas != null)
        {
            canvas.Fill(new Color(1f, 1f, 1f, 0f));
        }

        Image? result = plane.Render(canvas, width, height, scale, normal, useTexture);

        if (result != null && result != canvas)
        {
            result = (Image)result.Duplicate();
        }

        return new PlaneBitmapData(result, -1);
    }

    public override string GetTextureIdentifier(double scale, IVector3d? normal)
    {
        if (normal != null)
        {
            return scale + "_" + normal.X + "_" + normal.Y + "_" + normal.Z;
        }

        return base.GetTextureIdentifier(scale, normal);
    }

    protected virtual void ParseWalls(XElement wallsElement)
    {
        foreach (XElement wallElement in wallsElement.Elements("wall"))
        {
            string? id = (string?)wallElement.Attribute("id");

            if (string.IsNullOrEmpty(id))
            {
                continue;
            }

            WallPlane plane = new();
            ParseVisualizations(plane, wallElement.Elements("visualization"));

            if (!AddPlane(id, plane))
            {
                plane.Dispose();
            }
        }
    }
}
