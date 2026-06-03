namespace Vortex.Room.Renderer.Utils;

/// <summary>
/// Tracks mouse-over state per room object. Stores the currently hovered object ID and sprite tag.
/// </summary>
/// @see com.sulake.room.renderer.utils.ObjectMouseData (class_3727)
public class ObjectMouseData
{
    public string ObjectId { get; set; } = "";
    public string SpriteTag { get; set; } = "";
}
