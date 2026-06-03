using System;

using Vortex.Room.Object.Visualization.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureParticleSystemParticle
public class FurnitureParticleSystemParticle
{
    private double _lastX;
    private double _lastY;
    private double _lastZ;
    private int _lifeTime;
    private double _fadeTime;
    private IGraphicAsset[]? _frames;

    public bool Fade { get; private set; }

    public double AlphaMultiplier { get; private set; } = 1;

    public int Age { get; private set; }

    public bool IsEmitter { get; private set; }

    public bool IsAlive => Age <= _lifeTime;

    public double X { get; set; }

    public double Y { get; set; }

    public double Z { get; set; }

    public double LastX
    {
        get => _lastX;
        set
        {
            HasMoved = true;
            _lastX = value;
        }
    }

    public double LastY
    {
        get => _lastY;
        set
        {
            HasMoved = true;
            _lastY = value;
        }
    }

    public double LastZ
    {
        get => _lastZ;
        set
        {
            HasMoved = true;
            _lastZ = value;
        }
    }

    public bool HasMoved { get; private set; }

    public double DirectionX { get; private set; }

    public double DirectionY { get; private set; }

    public double DirectionZ { get; private set; }

    public void Init(
        double x,
        double y,
        double z,
        double dirX,
        double dirY,
        double dirZ,
        double speed,
        double timeStep,
        int lifeTime,
        bool isEmitter = false,
        IGraphicAsset[]? frames = null,
        bool fade = false)
    {
        X = x;
        Y = y;
        Z = z;
        DirectionX = dirX * speed;
        DirectionY = dirY * speed;
        DirectionZ = dirZ * speed;
        _lastX = X - DirectionX * timeStep;
        _lastY = Y - DirectionY * timeStep;
        _lastZ = Z - DirectionZ * timeStep;
        Age = 0;
        HasMoved = false;
        _lifeTime = lifeTime;
        IsEmitter = isEmitter;
        _frames = frames;
        Fade = fade;
        AlphaMultiplier = 1;
        _fadeTime = 0.5 + Random.Shared.NextDouble() * 0.5;
    }

    public virtual void Update()
    {
        Age++;

        if (Age == _lifeTime)
        {
            Ignite();
        }

        if (Fade && _lifeTime > 0)
        {
            double ratio = (double)Age / _lifeTime;

            if (ratio > _fadeTime)
            {
                AlphaMultiplier = (_lifeTime - Age) / (double)(_lifeTime * (1 - _fadeTime));
            }
        }
    }

    public IGraphicAsset? GetAsset()
    {
        if (_frames != null && _frames.Length > 0)
        {
            return _frames[Age % _frames.Length];
        }

        return null;
    }

    public virtual void Dispose()
    {
        _frames = null;
    }

    public void Copy(FurnitureParticleSystemParticle other, double scaleFactor)
    {
        X = other.X * scaleFactor;
        Y = other.Y * scaleFactor;
        Z = other.Z * scaleFactor;
        _lastX = other._lastX * scaleFactor;
        _lastY = other._lastY * scaleFactor;
        _lastZ = other._lastZ * scaleFactor;
        HasMoved = other.HasMoved;
        DirectionX = other.DirectionX;
        DirectionY = other.DirectionY;
        DirectionZ = other.DirectionZ;
        Age = other.Age;
        _lifeTime = other._lifeTime;
        IsEmitter = other.IsEmitter;
        Fade = other.Fade;
        _fadeTime = other._fadeTime;
        AlphaMultiplier = other.AlphaMultiplier;
    }

    protected virtual void Ignite()
    {
    }
}
