using System;

using Vortex.Room.Object.Visualization;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurniturePartyBeamerVisualization
public class FurniturePartyBeamerVisualization : AnimatedFurnitureVisualization
{
    private const int AREA_DIAMETER_SMALL = 15;
    private const int AREA_DIAMETER_LARGE = 31;
    private const int ANIM_SPEED_FAST = 2;
    private const int ANIM_SPEED_SLOW = 1;

    private double[]? _positions;
    private int[]? _directions;
    private int[]? _speeds;
    private double[]? _amplitudeFactors;
    private double[] _pointOffsetsX = new double[2];
    private double[] _pointOffsetsY = new double[2];

    protected override int UpdateAnimation(double scale)
    {
        if (_speeds == null)
        {
            InitItems(scale);
        }

        IRoomObjectSprite? sprite2 = GetSprite(2);

        if (sprite2 != null)
        {
            GetNewPoint(scale, 0);
        }

        IRoomObjectSprite? sprite3 = GetSprite(3);

        if (sprite3 != null)
        {
            GetNewPoint(scale, 1);
        }

        return base.UpdateAnimation(scale);
    }

    protected override int GetSpriteXOffset(int scale, int direction, int layer)
    {
        if ((layer == 2 || layer == 3) && _pointOffsetsX.Length == 2)
        {
            return (int)_pointOffsetsX[layer - 2];
        }

        return base.GetSpriteXOffset(scale, direction, layer);
    }

    protected override int GetSpriteYOffset(int scale, int direction, int layer)
    {
        if ((layer == 2 || layer == 3) && _pointOffsetsY.Length == 2)
        {
            return (int)_pointOffsetsY[layer - 2];
        }

        return base.GetSpriteYOffset(scale, direction, layer);
    }

    private void GetNewPoint(double scale, int index)
    {
        double position = _positions![index];
        int dir = _directions![index];
        int speed = _speeds![index];
        double amplitudeFactor = _amplitudeFactors![index];

        double speedScale = 1;
        int diameter;

        if (scale == 32)
        {
            diameter = AREA_DIAMETER_SMALL;
            speedScale = 0.5;
        }
        else
        {
            diameter = AREA_DIAMETER_LARGE;
        }

        double newPos = position + (dir * speed);

        if (Math.Abs(newPos) >= diameter)
        {
            if (dir > 0)
            {
                position -= newPos - diameter;
            }
            else
            {
                position += -diameter - newPos;
            }

            dir = -dir;
            _directions[index] = dir;
        }

        double amplitude = (diameter - Math.Abs(position)) * amplitudeFactor;
        double yOffset = dir * Math.Sin(Math.Abs(position / 4.0)) * amplitude;

        if (dir > 0)
        {
            yOffset -= amplitude;
        }
        else
        {
            yOffset += amplitude;
        }

        position += dir * speed * speedScale;
        _positions[index] = position;

        if ((int)yOffset == 0)
        {
            _amplitudeFactors[index] = GetRandomAmplitudeFactor();
        }

        _pointOffsetsX[index] = position;
        _pointOffsetsY[index] = yOffset;
    }

    private void InitItems(double scale)
    {
        int diameter = scale == 32 ? AREA_DIAMETER_SMALL : AREA_DIAMETER_LARGE;

        _positions = new double[]
        {
            Random.Shared.NextDouble() * diameter * 1.5,
            Random.Shared.NextDouble() * diameter * 1.5,
        };

        _directions = [1, -1];
        _speeds = [ANIM_SPEED_FAST, ANIM_SPEED_SLOW];

        _amplitudeFactors = new double[]
        {
            GetRandomAmplitudeFactor(),
            GetRandomAmplitudeFactor(),
        };
    }

    private static double GetRandomAmplitudeFactor()
    {
        return (Random.Shared.NextDouble() * 0.30) + 0.15;
    }
}
