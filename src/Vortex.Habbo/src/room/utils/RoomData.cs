using System.Xml.Linq;

namespace Vortex.Habbo.Room.Utils;

/// @see com.sulake.habbo.room.utils.RoomData
public class RoomData(int roomId, XElement? data)
{
    public int RoomId { get; } = roomId;

    public XElement? Data { get; } = data;

    public string? FloorType { get; set; }

    public string? WallType { get; set; }

    public string? LandscapeType { get; set; }
}
