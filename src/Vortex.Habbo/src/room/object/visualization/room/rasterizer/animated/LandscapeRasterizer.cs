using System;
using System.Linq;
using System.Xml.Linq;

using Godot;

using Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;
using Vortex.Habbo.Room.Object.Visualization.Room.Utils;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Animated;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.animated.LandscapeRasterizer
public class LandscapeRasterizer : PlaneRasterizer
{
    private const int UPDATE_INTERVAL = 500;

    private int _landscapeWidth;
    private int _landscapeHeight;

    public override bool InitializeDimensions(int width, int height)
    {
        if (width < 0)
        {
            width = 0;
        }

        if (height < 0)
        {
            height = 0;
        }

        _landscapeWidth = width;
        _landscapeHeight = height;

        return true;
    }

    protected override void InitializePlanes()
    {
        if (Data == null)
        {
            return;
        }

        XElement? landscapesElement = Data.Element("landscapes");

        if (landscapesElement != null)
        {
            ParseLandscapes(landscapesElement);
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
        LandscapePlane? plane = GetPlane(type) as LandscapePlane ?? GetPlane("default") as LandscapePlane;

        if (plane == null)
        {
            return null;
        }

        if (canvas != null)
        {
            canvas.Fill(Colors.White);
        }

        Image? result = plane.Render(
            canvas, width, height, scale, normal, useTexture,
            offsetX, offsetY, topAlignOffset, bottomAlignOffset, timeStamp);

        if (result != null && result != canvas)
        {
            result = (Image)result.Duplicate();
        }

        if (!plane.IsStatic((int)scale))
        {
            return new PlaneBitmapData(result, (int)((Math.Round((double)timeStamp / UPDATE_INTERVAL) * UPDATE_INTERVAL) + UPDATE_INTERVAL));
        }

        return new PlaneBitmapData(result, -1);
    }

    public override string GetTextureIdentifier(double scale, IVector3d? normal)
    {
        if (normal == null)
        {
            return base.GetTextureIdentifier(scale, normal);
        }

        if (normal.X < 0)
        {
            return scale + "_0";
        }

        return scale + "_1";

    }

    private void ParseLandscapes(XElement landscapesElement)
    {
        int seed = (int)(new Random().NextDouble() * 654321);

        foreach (XElement landscapeElement in landscapesElement.Elements("landscape"))
        {
            string? id = (string?)landscapeElement.Attribute("id");

            if (string.IsNullOrEmpty(id))
            {
                continue;
            }

            LandscapePlane plane = new();

            foreach (XElement vizElement in landscapeElement.Elements("animatedVisualization"))
            {
                XAttribute? sizeAttr = vizElement.Attribute("size");

                if (sizeAttr == null)
                {
                    continue;
                }

                int size = (int)sizeAttr;

                string? horizontalAngleStr = (string?)vizElement.Attribute("horizontalAngle");
                string? verticalAngleStr = (string?)vizElement.Attribute("verticalAngle");

                double horizontalAngle = LandscapePlane.HORIZONTAL_ANGLE_DEFAULT;

                if (!string.IsNullOrEmpty(horizontalAngleStr))
                {
                    horizontalAngle = double.Parse(horizontalAngleStr);
                }

                double verticalAngle = LandscapePlane.VERTICAL_ANGLE_DEFAULT;

                if (!string.IsNullOrEmpty(verticalAngleStr))
                {
                    verticalAngle = double.Parse(verticalAngleStr);
                }

                int layerCount = vizElement.Elements("visualizationLayer").Count()
                                 + vizElement.Elements("animationLayer").Count();

                PlaneVisualization? viz = plane.CreatePlaneVisualization(
                    size, layerCount, GetGeometry(size, horizontalAngle, verticalAngle));

                if (viz == null)
                {
                    continue;
                }

                Randomizer.SetSeed(seed);
                int layerIndex = 0;

                foreach (XElement childElement in vizElement.Elements())
                {
                    switch (childElement.Name.LocalName)
                    {
                        case "visualizationLayer":
                            ParseVisualizationLayer(viz, childElement, layerIndex);
                            break;
                        case "animationLayer":
                            ParseAnimationLayer(viz, childElement, layerIndex);
                            break;
                        default:
                            continue;
                    }

                    layerIndex++;
                }
            }

            if (!AddPlane(id, plane))
            {
                plane.Dispose();
            }
        }
    }

    private void ParseVisualizationLayer(PlaneVisualization viz, XElement layerElement, int index)
    {
        PlaneMaterial? material = null;
        int align = PlaneVisualizationLayer.ALIGN_TOP;

        string? materialId = (string?)layerElement.Attribute("materialId");

        if (!string.IsNullOrEmpty(materialId))
        {
            material = GetMaterial(materialId);
        }

        string? offsetStr = (string?)layerElement.Attribute("offset");
        int offset = 0;

        if (!string.IsNullOrEmpty(offsetStr))
        {
            offset = int.Parse(offsetStr);
        }

        string? colorStr = (string?)layerElement.Attribute("color");
        uint color = LandscapePlane.DEFAULT_COLOR;

        if (!string.IsNullOrEmpty(colorStr))
        {
            if (colorStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                color = uint.Parse(colorStr.AsSpan(2), System.Globalization.NumberStyles.HexNumber);
            }
            else
            {
                color = uint.Parse(colorStr);
            }
        }

        string? alignStr = (string?)layerElement.Attribute("align");

        align = alignStr switch
        {
            "bottom" => PlaneVisualizationLayer.ALIGN_BOTTOM,
            "top" => PlaneVisualizationLayer.ALIGN_TOP,
            _ => align,
        };

        viz.SetLayer(index, material, color, align, offset);
    }

    private void ParseAnimationLayer(PlaneVisualization viz, XElement animLayerElement, int index)
    {
        XElement animXml = new("animation");

        foreach (XElement animItemElement in animLayerElement.Elements("animationItem"))
        {
            string? itemId = (string?)animItemElement.Attribute("id");
            string? assetId = (string?)animItemElement.Attribute("assetId");

            if (string.IsNullOrEmpty(itemId) || string.IsNullOrEmpty(assetId))
            {
                continue;
            }

            double x = GetCoordinateValue(
                (string?)animItemElement.Attribute("x") ?? "",
                (string?)animItemElement.Attribute("randomX") ?? "");

            double y = GetCoordinateValue(
                (string?)animItemElement.Attribute("y") ?? "",
                (string?)animItemElement.Attribute("randomY") ?? "");

            double speedX = (double?)animItemElement.Attribute("speedX") ?? 0;
            double speedY = (double?)animItemElement.Attribute("speedY") ?? 0;

            animXml.Add(new XElement("item",
                new XAttribute("x", x),
                new XAttribute("y", y),
                new XAttribute("speedX", speedX),
                new XAttribute("speedY", speedY),
                new XAttribute("asset", assetId)));
        }

        viz.SetAnimationLayer(index, animXml, AssetCollection);
    }

    private static double GetCoordinateValue(string value, string randomValue)
    {
        double result = 0;

        if (value.Length > 0)
        {
            if (value[^1] == '%')
            {
                string numStr = value[..^1];
                result = double.Parse(numStr) / 100;
            }
        }

        if (randomValue.Length > 0)
        {
            double maxRandom = 10000;
            int[] randomValues = Randomizer.GetValues(1, 0, (int)maxRandom);
            double randomFraction = randomValues[0] / maxRandom;

            if (randomValue[^1] == '%')
            {
                string numStr = randomValue[..^1];
                result += randomFraction * double.Parse(numStr) / 100;
            }
        }

        return result;
    }
}
