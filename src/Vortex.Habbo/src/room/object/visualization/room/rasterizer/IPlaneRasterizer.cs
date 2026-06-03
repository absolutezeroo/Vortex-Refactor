using Godot;

using Vortex.Habbo.Room.Object.Visualization.Room.Utils;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.class_3625
public interface IPlaneRasterizer
{
    bool InitializeDimensions(int width, int height);

    PlaneBitmapData? Render(
        Image? canvas, string type,
        double width, double height, double scale,
        IVector3d normal, bool useTexture,
        double offsetX = 0, double offsetY = 0,
        double topAlignOffset = 0, double bottomAlignOffset = 0,
        int timeStamp = 0);

    string GetTextureIdentifier(double scale, IVector3d? normal);

    object[]? GetLayers(string type);

    void Reinitialize();
}
