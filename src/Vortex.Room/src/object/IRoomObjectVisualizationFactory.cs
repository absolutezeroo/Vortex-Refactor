using System.Xml.Linq;

using Vortex.Room.Object.Visualization;
using Vortex.Room.Object.Visualization.Utils;

namespace Vortex.Room.Object;

/// @see com.sulake.room.object.IRoomObjectVisualizationFactory
public interface IRoomObjectVisualizationFactory
{
    IRoomObjectGraphicVisualization? CreateRoomObjectVisualization(string type);
    IGraphicAssetCollection? CreateGraphicAssetCollection();
    IRoomObjectVisualizationData? GetRoomObjectVisualizationData(string type, string visualization, XElement? xml);
}
