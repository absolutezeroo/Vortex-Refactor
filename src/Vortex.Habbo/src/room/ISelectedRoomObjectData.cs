using Vortex.Room.Utils;

namespace Vortex.Habbo.Room;

/// @see com.sulake.habbo.room.ISelectedRoomObjectData
public interface ISelectedRoomObjectData
{
    int Id { get; }

    int Category { get; }

    string Operation { get; }

    IVector3d? Loc { get; }

    IVector3d? Dir { get; }

    int TypeId { get; }

    string? InstanceData { get; }

    IStuffData? StuffData { get; }

    int State { get; }

    int AnimFrame { get; }

    string? Posture { get; }
}
