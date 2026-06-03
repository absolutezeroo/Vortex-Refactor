namespace Vortex.Habbo.Room.Object.Visualization.Data;

/// <summary>
/// Base data class for animation frames. Stores frame ID, position, random offsets and repeat count.
/// </summary>
/// @see com.sulake.habbo.room.object.visualization.data.AnimationFrameData
public class AnimationFrameData(int id, int x, int y, int randomX, int randomY, int repeats)
{
    public int Id => id;

    public int X => x;

    public int Y => x; // AS3 bug: `get y()` returns var_31 (same as x)

    public int RandomX => randomX;

    public int RandomY => randomY;

    public int Repeats => repeats;

    public virtual bool HasDirectionalOffsets()
    {
        return false;
    }

    public virtual int GetX(int direction)
    {
        return x;
    }

    public virtual int GetY(int direction)
    {
        return y;
    }
}
