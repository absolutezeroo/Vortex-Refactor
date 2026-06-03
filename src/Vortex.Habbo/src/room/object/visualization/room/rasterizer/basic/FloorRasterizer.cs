using System.Xml.Linq;

using Godot;

using Vortex.Habbo.Room.Object.Visualization.Room.Utils;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.basic.FloorRasterizer
public class FloorRasterizer : PlaneRasterizer
{
    protected override void InitializePlanes()
    {
        if (Data == null)
        {
            return;
        }

        XElement? floorsElement = Data.Element("floors");

        if (floorsElement != null)
        {
            ParseFloors(floorsElement);
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
        FloorPlane? plane = GetPlane(type) as FloorPlane ?? GetPlane("default") as FloorPlane;

        if (plane == null)
        {
            return null;
        }

        if (canvas != null)
        {
            canvas.Fill(new Color(1f, 1f, 1f, 0f));
        }

        Image? result = plane.Render(canvas, width, height, scale, normal, useTexture, offsetX, offsetY);

        if (result != null && result != canvas)
        {
            result = (Image)result.Duplicate();
        }

        return new PlaneBitmapData(result, -1);
    }

    private void ParseFloors(XElement floorsElement)
    {
        foreach (XElement floorElement in floorsElement.Elements("floor"))
        {
            string? id = (string?)floorElement.Attribute("id");

            if (string.IsNullOrEmpty(id))
            {
                continue;
            }

            FloorPlane plane = new();
            ParseVisualizations(plane, floorElement.Elements("visualization"));

            if (!AddPlane(id, plane))
            {
                plane.Dispose();
            }
        }
    }
}
