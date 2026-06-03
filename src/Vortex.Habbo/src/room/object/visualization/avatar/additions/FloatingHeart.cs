using Godot;

using Vortex.Core.Assets;
using Vortex.Room.Object.Visualization;

namespace Vortex.Habbo.Room.Object.Visualization.Avatar.Additions;

/// @see com.sulake.habbo.room.object.visualization.avatar.additions.FloatingHeart
public class FloatingHeart : ExpressionAddition
{
    private const int DELAY_BEFORE_ANIMATION = 300;
    private const int STATE_DELAY = 0;
    private const int STATE_FADE_IN = 1;
    private const int STATE_FLOAT = 2;
    private const int STATE_COMPLETE = 3;

    private BitmapDataAsset? _asset;
    private int _startTime;
    private double _progress;
    private int _offsetY;
    private double _scale;
    private int _state = -1;

    public FloatingHeart(int id, int type, AvatarVisualization visualization)
        : base(id, type, visualization)
    {
        _startTime = System.Environment.TickCount;
        _state = STATE_DELAY;
    }

    public override bool Animate(IRoomObjectSprite sprite)
    {
        double fadeCurve;
        int floatDistance;

        if (sprite == null)
        {
            return false;
        }

        if (_asset != null)
        {
            sprite.Asset = _asset.Content as Image;
        }

        if (_state == STATE_DELAY)
        {
            if (System.Environment.TickCount - _startTime < DELAY_BEFORE_ANIMATION)
            {
                return false;
            }

            _state = STATE_FADE_IN;
            sprite.Alpha = 0;
            sprite.Visible = true;
            _progress = 0;
            return true;
        }

        if (_state == STATE_FADE_IN)
        {
            _progress += 0.1;
            sprite.OffsetY = _offsetY;
            sprite.Alpha = (int)(System.Math.Pow(_progress, 0.9) * 255);

            if (_progress >= 1)
            {
                _progress = 0;
                sprite.Alpha = 255;
                _state = STATE_FLOAT;
            }

            return true;
        }

        if (_state == STATE_FLOAT)
        {
            fadeCurve = System.Math.Pow(_progress, 0.9);
            _progress += 0.05;
            floatDistance = _scale < 48 ? -30 : -40;
            sprite.OffsetY = _offsetY + (int)((_progress < 1 ? fadeCurve : 1) * floatDistance);
            sprite.Alpha = (int)((1 - fadeCurve) * 255);

            if (sprite.Alpha <= 0)
            {
                sprite.Visible = false;
                _state = STATE_COMPLETE;
            }

            return true;
        }

        return false;
    }

    public override void Update(IRoomObjectSprite sprite, double scale)
    {
        int offsetX;

        if (sprite == null)
        {
            return;
        }

        _scale = scale;
        int baseSize = 64;

        if (scale < 48)
        {
            _asset = _visualization!.GetAvatarRendererAsset("user_blowkiss_small_png") as BitmapDataAsset;

            if (_visualization.Angle == 90 || _visualization.Angle == 270)
            {
                offsetX = 0;
            }
            else if (_visualization.Angle == 135 || _visualization.Angle == 180 || _visualization.Angle == 225)
            {
                offsetX = 6;
            }
            else
            {
                offsetX = -6;
            }

            _offsetY = -38;
            baseSize = 32;
        }
        else
        {
            _asset = _visualization!.GetAvatarRendererAsset("user_blowkiss_png") as BitmapDataAsset;

            if (_visualization.Angle == 90 || _visualization.Angle == 270)
            {
                offsetX = -3;
            }
            else if (_visualization.Angle == 135 || _visualization.Angle == 180 || _visualization.Angle == 225)
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
            _offsetY += baseSize;
        }

        if (_asset != null)
        {
            sprite.Asset = _asset.Content as Image;
            sprite.OffsetX = offsetX;
            sprite.OffsetY = _offsetY;
            sprite.RelativeDepth = -0.02;
            sprite.Alpha = 0;
            double savedProgress = _progress;
            Animate(sprite);
            _progress = savedProgress;
        }
    }
}
