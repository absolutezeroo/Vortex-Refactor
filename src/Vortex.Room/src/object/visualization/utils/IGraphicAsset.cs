using Vortex.Core.Assets;

namespace Vortex.Room.Object.Visualization.Utils;

/// @see com.sulake.room.object.visualization.utils.IGraphicAsset
public interface IGraphicAsset
{
    bool FlipV { get; }
    bool FlipH { get; }
    int Width { get; }
    int Height { get; }
    IAsset? Asset { get; }
    string? AssetName { get; }
    string? LibraryAssetName { get; }
    int OffsetX { get; }
    int OffsetY { get; }
    int OriginalOffsetX { get; }
    int OriginalOffsetY { get; }
    bool UsesPalette { get; }
}
