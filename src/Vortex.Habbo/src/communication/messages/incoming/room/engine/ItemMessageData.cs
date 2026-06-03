namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;

/// @see com.sulake.habbo.communication.messages.incoming.room.engine.class_1710
public class ItemMessageData(int id, int type, bool isOldFormat)
{
    private bool _readOnly;

    public int Id => id;
    public int Type { get; set; } = type;
    public bool IsOldFormat => isOldFormat;
    public int WallX { get; set; }
    public int WallY { get; set; }
    public int LocalX { get; set; }
    public int LocalY { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public string Dir { get; set; } = "";
    public int State { get; set; }
    public string Data { get; set; } = "";
    public int UsagePolicy { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = "";
    public int SecondsToExpiration { get; set; }

    public void SetReadOnly()
    {
        _readOnly = true;
    }
}
