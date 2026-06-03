using Godot;

namespace Vortex.Room.Object.Visualization;

/// @see com.sulake.room.object.visualization.IRoomObjectSprite
public interface IRoomObjectSprite
{
    Image? Asset { get; set; }
    string? AssetName { get; set; }
    string? LibraryAssetName { get; set; }
    string? AssetPosture { get; set; }
    bool Visible { get; set; }
    string? Tag { get; set; }
    int Alpha { get; set; }
    int Color { get; set; }
    string? BlendMode { get; set; }
    object[]? Filters { get; set; }
    bool FlipH { get; set; }
    bool FlipV { get; set; }
    int Direction { get; set; }
    int OffsetX { get; set; }
    int OffsetY { get; set; }
    int Width { get; }
    int Height { get; }
    double RelativeDepth { get; set; }
    bool VaryingDepth { get; set; }
    bool ClickHandling { get; set; }
    bool SkipMouseHandling { get; set; }
    int InstanceId { get; }
    int UpdateId { get; }
    int SpriteType { get; set; }
    string? ObjectType { get; set; }
    int AlphaTolerance { get; set; }
    int PlaneId { get; set; }
}
