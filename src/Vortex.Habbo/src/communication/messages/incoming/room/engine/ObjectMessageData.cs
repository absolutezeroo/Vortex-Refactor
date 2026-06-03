using Vortex.Habbo.Room;
using Vortex.Habbo.Room.Object.Data;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;

/// @see com.sulake.habbo.communication.messages.incoming.room.engine.class_1765
public class ObjectMessageData(int id)
{
    private bool _readOnly;

    public int Id => id;
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public int Dir { get; set; }
    public int SizeX { get; set; }
    public int SizeY { get; set; }
    public double SizeZ { get; set; }
    public int Type { get; set; }
    public int Extra { get; set; } = -1;
    public int State { get; set; }
    public IStuffData Data { get; set; } = new LegacyStuffData();
    public int UsagePolicy { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = "";
    public string? StaticClass { get; set; }
    public bool TrustedSender { get; set; }
    public int ExpiryTime { get; set; }

    public void SetReadOnly()
    {
        _readOnly = true;
    }
}
