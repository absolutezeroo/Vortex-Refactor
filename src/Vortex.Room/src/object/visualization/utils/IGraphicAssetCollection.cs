using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;

namespace Vortex.Room.Object.Visualization.Utils;

/// @see com.sulake.room.object.visualization.utils.IGraphicAssetCollection (class_3367)
public interface IGraphicAssetCollection
{
    void Dispose();
    IAssetLibrary? AssetLibrary { set; }
    void AddReference();
    void RemoveReference();
    int ReferenceCount { get; }
    int LastReferenceTimeStamp { get; }
    bool Define(XElement xml);
    IGraphicAsset? GetAsset(string name);
    IGraphicAsset? GetAssetWithPalette(string name, string palette);
    string[]? GetPaletteNames();
    int[]? GetPaletteColors(string name);
    XElement? GetPaletteXml(string name);
    bool AddAsset(string name, Image data, bool overwrite, int offsetX = 0, int offsetY = 0, bool flipH = false, bool flipV = false);
    void DisposeAsset(string name);
}
