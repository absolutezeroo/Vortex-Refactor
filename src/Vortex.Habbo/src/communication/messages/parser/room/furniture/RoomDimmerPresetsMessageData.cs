namespace Vortex.Habbo.Communication.Messages.Parser.Room.Furniture;

/// @see com.sulake.habbo.communication.messages.incoming.room.furniture.RoomDimmerPresetsMessageData
public class RoomDimmerPresetsMessageData(int id)
{
    private bool _readOnly;

    public int Id => id;
    public int Type { get; set; }
    public int Color { get; set; }
    public int Light { get; set; }

    public void SetReadOnly()
    {
        _readOnly = true;
    }
}
