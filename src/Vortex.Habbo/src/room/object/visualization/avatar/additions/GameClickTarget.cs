using Godot;

using Vortex.Room.Object.Visualization;

namespace Vortex.Habbo.Room.Object.Visualization.Avatar.Additions;

/// @see com.sulake.habbo.room.object.visualization.avatar.additions.GameClickTarget
public class GameClickTarget(int id) : IAvatarAddition
{
    private const int WIDTH = 46;
    private const int HEIGHT = 60;
    private const int OFFSET_X = -23;
    private const int OFFSET_Y = -48;

    private Image? _bitmap;

    public bool disposed { get; private set; }

    public int Id { get; } = id;

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        _bitmap = null;
        disposed = true;
    }

    public bool Animate(IRoomObjectSprite sprite)
    {
        return false;
    }

    public void Update(IRoomObjectSprite? sprite, double scale)
    {
        if (sprite == null)
        {
            return;
        }

        if (_bitmap == null)
        {
            _bitmap = Image.CreateEmpty(WIDTH, HEIGHT, false, Image.Format.Rgba8);
        }

        sprite.Visible = true;
        sprite.Asset = _bitmap;
        sprite.OffsetX = OFFSET_X;
        sprite.OffsetY = OFFSET_Y;
        sprite.AlphaTolerance = -1;
    }
}
