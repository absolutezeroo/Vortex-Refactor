using Godot;

using Vortex.Core.Assets;
using Vortex.Room.Object.Visualization;

namespace Vortex.Habbo.Room.Object.Visualization.Avatar.Additions;

/// @see com.sulake.habbo.room.object.visualization.avatar.additions.FloatingIdleZ
public class FloatingIdleZ : IAvatarAddition
{
    private const int DELAY_BEFORE_ANIMATION = 2000;
    private const int DELAY_PER_FRAME = 2000;
    private const int STATE_DELAY = 0;
    private const int STATE_FRAME_A = 1;
    private const int STATE_FRAME_B = 2;

    protected int _id;
    protected AvatarVisualization? _visualization;

    private BitmapDataAsset? _asset;
    private int _startTime;
    private int _offsetY;
    private double _scale;
    private int _state = -1;

    public FloatingIdleZ(int id, AvatarVisualization visualization)
    {
        _id = id;
        _visualization = visualization;
        _startTime = System.Environment.TickCount;
        _state = STATE_DELAY;
    }

    public int Id => _id;

    public bool disposed => _visualization == null;

    public void Dispose()
    {
        _visualization = null;
        _asset = null;
    }

    public bool Animate(IRoomObjectSprite sprite)
    {
        if (sprite == null)
        {
            return false;
        }

        if (_state == STATE_DELAY)
        {
            if (System.Environment.TickCount - _startTime >= DELAY_BEFORE_ANIMATION)
            {
                _state = STATE_FRAME_A;
                _startTime = System.Environment.TickCount;
                _asset = _visualization!.GetAvatarRendererAsset(GetAssetNameForFrame(1)) as BitmapDataAsset;
            }
        }

        if (_state == STATE_FRAME_A)
        {
            if (System.Environment.TickCount - _startTime >= DELAY_PER_FRAME)
            {
                _state = STATE_FRAME_B;
                _startTime = System.Environment.TickCount;
                _asset = _visualization!.GetAvatarRendererAsset(GetAssetNameForFrame(2)) as BitmapDataAsset;
            }
        }

        if (_state == STATE_FRAME_B)
        {
            if (System.Environment.TickCount - _startTime >= DELAY_PER_FRAME)
            {
                _state = STATE_FRAME_A;
                _startTime = System.Environment.TickCount;
                _asset = _visualization!.GetAvatarRendererAsset(GetAssetNameForFrame(1)) as BitmapDataAsset;
            }
        }

        if (_asset != null)
        {
            sprite.Asset = _asset.Content as Image;
            sprite.Alpha = 255;
            sprite.Visible = true;
        }
        else
        {
            sprite.Visible = false;
        }

        return false;
    }

    public void Update(IRoomObjectSprite sprite, double scale)
    {
        int offsetX;

        if (sprite == null)
        {
            return;
        }

        _scale = scale;
        _asset = _visualization!.GetAvatarRendererAsset(GetAssetNameForFrame(_state == STATE_FRAME_A ? 1 : 2)) as BitmapDataAsset;

        int baseSize = 64;

        if (scale < 48)
        {
            if (_visualization.Angle == 135 || _visualization.Angle == 180 || _visualization.Angle == 225 || _visualization.Angle == 270)
            {
                offsetX = 10;
            }
            else
            {
                offsetX = -16;
            }

            _offsetY = -38;
            baseSize = 32;
        }
        else
        {
            if (_visualization.Angle == 135 || _visualization.Angle == 180 || _visualization.Angle == 225 || _visualization.Angle == 270)
            {
                offsetX = 22;
            }
            else
            {
                offsetX = -30;
            }

            _offsetY = -70;
        }

        if (_visualization.Posture == "sit")
        {
            _offsetY += baseSize / 2;
        }
        else if (_visualization.Posture == "lay")
        {
            _offsetY += (int)(baseSize - 0.3 * baseSize);
        }

        if (_asset != null)
        {
            sprite.Asset = _asset.Content as Image;
            sprite.OffsetX = offsetX;
            sprite.OffsetY = _offsetY;
            sprite.RelativeDepth = -0.02;
            sprite.Alpha = 0;
        }
    }

    protected string GetAssetNameForFrame(int frame)
    {
        string direction = "left";

        if (_visualization!.Angle == 135 || _visualization.Angle == 180 || _visualization.Angle == 225 || _visualization.Angle == 270)
        {
            direction = "right";
        }

        return "user_idle_" + direction + "_" + frame + (_scale < 48 ? "_small" : "") + "_png";
    }
}
