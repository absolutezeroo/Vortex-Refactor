using Vortex.Room.Utils;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;

/// @see com.sulake.habbo.communication.messages.incoming.room.engine.class_1693
public class SlideObjectMessageData(int id, Vector3d loc, Vector3d target, string? moveType = null)
{
    public const string MOVE = "mv";
    public const string SLIDE = "sld";

    public int Id => id;
    public Vector3d Loc { get; set; } = loc;
    public Vector3d Target { get; set; } = target;
    public string? MoveType { get; set; } = moveType;
}
