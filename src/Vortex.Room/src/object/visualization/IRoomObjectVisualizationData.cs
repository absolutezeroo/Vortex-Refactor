using System.Xml.Linq;

namespace Vortex.Room.Object.Visualization;

/// @see com.sulake.room.object.visualization.IRoomObjectVisualizationData
public interface IRoomObjectVisualizationData
{
    bool Initialize(XElement xml);
    void Dispose();
}
