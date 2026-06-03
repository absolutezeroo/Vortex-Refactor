namespace Vortex.Habbo.Room.Object.Visualization.Data;

/// <summary>
/// Extends AnimationFrameData with per-direction offsets via DirectionalOffsetData.
/// </summary>
/// @see com.sulake.habbo.room.object.visualization.data.AnimationFrameDirectionalData
public class AnimationFrameDirectionalData
(
    int id,
    int x,
    int y,
    int randomX,
    int randomY,
    DirectionalOffsetData? directionalOffsets,
    int repeats
)
    : AnimationFrameData(id, x, y, randomX, randomY, repeats)
{
    public override bool HasDirectionalOffsets()
    {
        return directionalOffsets != null;
    }

    public override int GetX(int direction)
    {
        if (directionalOffsets != null)
        {
            return directionalOffsets.GetOffsetX(direction, base.GetX(direction));
        }

        return base.GetX(direction);
    }

    public override int GetY(int direction)
    {
        if (directionalOffsets != null)
        {
            return directionalOffsets.GetOffsetY(direction, base.GetY(direction));
        }

        return base.GetY(direction);
    }
}
