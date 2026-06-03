using Vortex.Room.Object.Visualization.Utils;

namespace Vortex.Room.Object.Visualization;

/// @see com.sulake.room.object.visualization.IRoomObjectGraphicVisualization
public interface IRoomObjectGraphicVisualization : IRoomObjectVisualization
{
    IGraphicAssetCollection? AssetCollection { get; set; }
    void SetExternalBaseUrls(string baseUrl, string baseUrlSecure, bool useSecure);
}
