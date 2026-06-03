using System.Globalization;
using System.Xml.Linq;

using Godot;

using Vortex.Habbo.Room.Object.Visualization.Room.Utils;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.basic.WallAdRasterizer
public class WallAdRasterizer : WallRasterizer
{
    public override string GetTextureIdentifier(double scale, IVector3d? normal)
    {
        return scale.ToString(CultureInfo.CurrentCulture);
    }

    protected override void InitializePlanes()
    {
        if (Data == null)
        {
            return;
        }

        XElement? wallAdsElement = Data.Element("wallAds");

        if (wallAdsElement != null)
        {
            ParseWalls(wallAdsElement);
        }
    }

    protected override void ParseWalls(XElement wallsElement)
    {
        foreach (XElement wallAdElement in wallsElement.Elements("wallAd"))
        {
            string? id = (string?)wallAdElement.Attribute("id");

            if (string.IsNullOrEmpty(id))
            {
                continue;
            }

            WallPlane plane = new();
            ParseVisualizations(plane, wallAdElement.Elements("visualization"));

            if (GetPlane(id) == null)
            {
                AddPlane(id, plane);
            }
            else
            {
                plane.Dispose();
            }
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
}
