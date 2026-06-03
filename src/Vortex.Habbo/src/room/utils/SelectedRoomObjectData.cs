using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Utils;

/// @see com.sulake.habbo.room.utils.SelectedRoomObjectData
public class SelectedRoomObjectData : ISelectedRoomObjectData
{
    private Vector3d? _loc;
    private Vector3d? _dir;

    public int Id { get; }
    public int Category { get; }
    public string Operation { get; }
    public IVector3d? Loc => _loc;
    public IVector3d? Dir => _dir;
    public int TypeId { get; }
    public string? InstanceData { get; }
    public IStuffData? StuffData { get; }
    public int State { get; }
    public int AnimFrame { get; }
    public string? Posture { get; }

    public SelectedRoomObjectData(
        int id,
        int category,
        string operation,
        IVector3d loc,
        IVector3d dir,
        int typeId = 0,
        string? instanceData = null,
        IStuffData? stuffData = null,
        int state = -1,
        int animFrame = -1,
        string? posture = null)
    {
        Id = id;
        Category = category;
        Operation = operation;
        _loc = new Vector3d();
        _loc.Assign(loc);
        _dir = new Vector3d();
        _dir.Assign(dir);
        TypeId = typeId;
        InstanceData = instanceData;
        StuffData = stuffData;
        State = state;
        AnimFrame = animFrame;
        Posture = posture;
    }

    public void Dispose()
    {
        _loc = null;
        _dir = null;
    }
}
