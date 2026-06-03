using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Utils;

/// @see com.sulake.habbo.room.utils.FurnitureData
public class FurnitureData
{
    private readonly Vector3d _loc = new();
    private readonly Vector3d _dir = new();

    public int Id { get; }
    public int TypeId { get; }
    public string? Type { get; }
    public IVector3d Loc => _loc;
    public IVector3d Dir => _dir;
    public int State { get; }
    public IStuffData? Data { get; }
    public double Extra { get; }
    public int ExpiryTime { get; }
    public int UsagePolicy { get; }
    public int OwnerId { get; }
    public string OwnerName { get; }
    public bool Synchronized { get; }
    public bool RealRoomObject { get; }
    public double SizeZ { get; }
    public string? WallData { get; }

    public FurnitureData(
        int id,
        int typeId,
        string? type,
        IVector3d loc,
        IVector3d dir,
        int state,
        IStuffData? data,
        double extra = double.NaN,
        int expiryTime = -1,
        int usagePolicy = 0,
        int ownerId = 0,
        string ownerName = "",
        bool synchronized = true,
        bool realRoomObject = true,
        double sizeZ = -1,
        string? wallData = null)
    {
        Id = id;
        TypeId = typeId;
        Type = type;
        _loc.Assign(loc);
        _dir.Assign(dir);
        State = state;
        Data = data;
        Extra = extra;
        ExpiryTime = expiryTime;
        UsagePolicy = usagePolicy;
        OwnerId = ownerId;
        OwnerName = ownerName;
        Synchronized = synchronized;
        RealRoomObject = realRoomObject;
        SizeZ = sizeZ;
        WallData = wallData;
    }
}
