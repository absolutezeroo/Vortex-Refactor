using Godot;

using Vortex.Core.Assets;
using Vortex.Room.Object.Visualization;

namespace Vortex.Habbo.Room.Object.Visualization.Avatar.Additions;

/// @see com.sulake.habbo.room.object.visualization.avatar.additions.MutedBubble
public class MutedBubble : IAvatarAddition
{
    private BitmapDataAsset? _asset;
    private AvatarVisualization? _visualization;
    private double _relativeDepth;

    public MutedBubble(int id, AvatarVisualization visualization)
    {
        Id = id;
        _visualization = visualization;
    }

    public double RelativeDepth
    {
        set => _relativeDepth = value;
    }

    public int Id { get; } = -1;

    public bool disposed => _visualization == null;

    public void Dispose()
    {
        _visualization = null;
        _asset = null;
    }

    public bool Animate(IRoomObjectSprite? sprite)
    {
        if (_asset != null && sprite != null)
        {
            sprite.Asset = _asset.Content as Image;
        }

        return false;
    }

    public void Update(IRoomObjectSprite? sprite, double scale)
    {
        int offsetX = 0;
        int offsetY = 0;

        if (sprite == null)
        {
            return;
        }

        sprite.Visible = true;
        sprite.RelativeDepth = _relativeDepth;
        sprite.Alpha = 255;

        int baseSize = 64;

        if (scale < 48)
        {
            _asset = _visualization!.GetAvatarRendererAsset("user_muted_small_png") as BitmapDataAsset;
            offsetX = -12;
            offsetY = -66;
            baseSize = 32;
        }
        else
        {
            _asset = _visualization!.GetAvatarRendererAsset("user_muted_png") as BitmapDataAsset;
            offsetX = -15;
            offsetY = -110;
        }

        switch (_visualization.Posture)
        {
            case "sit":
                offsetY += baseSize / 2;

                break;
            case "lay":
                offsetY += baseSize;

                break;
        }

        if (_asset == null)
        {
            return;
        }

        sprite.Asset = _asset.Content as Image;
        sprite.OffsetX = offsetX;
        sprite.OffsetY = offsetY;
        sprite.RelativeDepth = -0.02;
    }
}
