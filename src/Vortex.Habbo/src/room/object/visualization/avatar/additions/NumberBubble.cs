using Godot;

using Vortex.Core.Assets;
using Vortex.Room.Object.Visualization;

namespace Vortex.Habbo.Room.Object.Visualization.Avatar.Additions;

/// @see com.sulake.habbo.room.object.visualization.avatar.additions.NumberBubble
public class NumberBubble : IAvatarAddition
{
    private AvatarVisualization? _visualization;
    private BitmapDataAsset? _asset;
    private double _scale;
    private int _numberValue;
    private int _fadeDirection;
    private bool _animating;
    private int _frameCounter;

    public NumberBubble(int id, int number, AvatarVisualization visualization)
    {
        Id = id;
        _numberValue = number;
        _visualization = visualization;
    }

    public int Id { get; } = -1;

    public bool disposed => _visualization == null;

    public void Dispose()
    {
        _visualization = null;
        _asset = null;
    }

    public void Update(IRoomObjectSprite sprite, double scale)
    {

        if (sprite == null)
        {
            return;
        }

        _scale = scale;

        if (_numberValue > 0)
        {
            int baseSize = 64;

            int offsetX;
            int offsetY;

            if (scale < 48)
            {
                _asset = _visualization!.GetAvatarRendererAsset("number_" + _numberValue + "_small_png") as BitmapDataAsset;
                offsetX = -6;
                offsetY = -52;
                baseSize = 32;
            }
            else
            {
                _asset = _visualization!.GetAvatarRendererAsset("number_" + _numberValue + "_png") as BitmapDataAsset;
                offsetX = -8;
                offsetY = -105;
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

            if (_asset != null)
            {
                sprite.Visible = true;
                sprite.Asset = _asset.Content as Image;
                sprite.OffsetX = offsetX;
                sprite.OffsetY = offsetY;
                sprite.RelativeDepth = -0.01;
                _fadeDirection = 1;
                _animating = true;
                _frameCounter = 0;
                sprite.Alpha = 0;
            }
            else
            {
                sprite.Visible = false;
            }
        }
        else if (sprite.Visible)
        {
            _fadeDirection = -1;
        }
    }

    public bool Animate(IRoomObjectSprite? sprite)
    {
        if (sprite == null)
        {
            return false;
        }

        if (_asset != null)
        {
            sprite.Asset = _asset.Content as Image;
        }

        int alpha = sprite.Alpha;
        bool drifted = false;

        if (_animating)
        {
            _frameCounter++;

            if (_frameCounter < 10)
            {
                return false;
            }

            if (_fadeDirection < 0)
            {
                if (_scale < 48)
                {
                    sprite.OffsetY -= 2;
                }
                else
                {
                    sprite.OffsetY -= 4;
                }
            }
            else
            {
                int interval = 4;

                if (_scale < 48)
                {
                    interval = 8;
                }

                if (_frameCounter % interval == 0)
                {
                    sprite.OffsetY -= 1;
                    drifted = true;
                }
            }
        }

        switch (_fadeDirection)
        {
            case > 0:
                {
                    if (alpha < 255)
                    {
                        alpha += 32;
                    }

                    if (alpha >= 255)
                    {
                        alpha = 255;
                        _fadeDirection = 0;
                    }

                    sprite.Alpha = alpha;

                    return true;
                }
            case < 0:
                {
                    if (alpha >= 0)
                    {
                        alpha -= 32;
                    }

                    if (alpha <= 0)
                    {
                        _fadeDirection = 0;
                        _animating = false;
                        alpha = 0;
                        sprite.Visible = false;
                    }

                    sprite.Alpha = alpha;

                    return true;
                }
            default:
                return drifted;
        }

    }
}
