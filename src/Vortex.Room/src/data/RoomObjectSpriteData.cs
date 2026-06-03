namespace Vortex.Room.Data;

/// <summary>
/// Data carrier for room object sprite rendering properties.
/// </summary>
/// @see com.sulake.room.data.RoomObjectSpriteData
public class RoomObjectSpriteData
{
    public int ObjectId { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public double Z { get; set; }

    public string? Name { get; set; }

    public string? BlendMode { get; set; }

    public bool FlipH { get; set; }

    public double Skew { get; set; }

    public bool Frame { get; set; }

    public string? Color { get; set; }

    public double Alpha { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public string? ObjectType { get; set; }

    public string? Posture { get; set; }
}
