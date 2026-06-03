using System.Xml.Linq;

using Vortex.Core.Assets;
using Vortex.Room.Object;
using Vortex.Room.Object.Visualization.Utils;

namespace Vortex.Room;

/// @see com.sulake.room.IRoomContentLoader
public interface IRoomContentLoader
{
    void Dispose();
    string? GetPlaceHolderType(string type);
    string[]? GetPlaceHolderTypes();
    string? GetContentType(string type);
    bool HasInternalContent(string type);
    bool LoadObjectContent(string type, object? eventDispatcher);
    bool InsertObjectContent(int category, int objectId, IAssetLibrary library);
    string? GetVisualizationType(string type);
    string? GetLogicType(string type);
    bool HasVisualizationXml(string type);
    XElement? GetVisualizationXml(string type);
    bool HasAssetXml(string type);
    XElement? GetAssetXml(string type);
    bool HasLogicXml(string type);
    XElement? GetLogicXml(string type);
    IGraphicAssetCollection? GetGraphicAssetCollection(string type);
    void RoomObjectCreated(IRoomObject obj, string type);
}
