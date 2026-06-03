using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object;

/// <summary>
/// Bitmap mask metadata for a plane (e.g., window cutout, hole).
/// </summary>
/// @see com.sulake.habbo.room.object.RoomPlaneBitmapMaskData
public class RoomPlaneBitmapMaskData
{
    public const string MASK_CATEGORY_WINDOW = "window";
    public const string MASK_CATEGORY_HOLE = "hole";

    private Vector3d? _loc;

    public RoomPlaneBitmapMaskData(string type, IVector3d loc, string category)
    {
        Type = type;
        Loc = loc;
        Category = category;
    }

    public IVector3d? Loc
    {
        get => _loc;
        set
        {
            _loc ??= new Vector3d();

            _loc.Assign(value!);
        }
    }

    public string? Type { get; set; }

    public string? Category { get; set; }

    public void Dispose()
    {
        _loc = null;
    }
}
