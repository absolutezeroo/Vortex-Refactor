using System;

using Vortex.Room.Object.Visualization.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureParticleSystemEmitter
public class FurnitureParticleSystemEmitter(string name = "", int spriteId = -1) : FurnitureParticleSystemParticle
{
    public const string SHAPE_CONE = "cone";
    public const string SHAPE_PLANE = "plane";
    public const string SHAPE_SPHERE = "sphere";

    private readonly string _name = name;
    private double _energy;
    private double _directionX;
    private double _directionY;
    private double _directionZ;
    private double _timeStep = 0.1;
    private double _gravity;
    private double _airFriction;
    private string _shape = SHAPE_CONE;
    private List<ParticleConfig> _particleConfigs = new();
    private int _maxParticles;
    private int _particlesPerFrame;
    private int _totalSpawned;
    private int _fuseTime = 10;
    private double _initialSpeed = 1;
    private int _burstPulse = 1;

    public List<FurnitureParticleSystemParticle> Particles { get; private set; } = new();

    public bool HasIgnited { get; private set; }

    public int RoomObjectSpriteId { get; } = spriteId;

    public override void Dispose()
    {
        foreach (FurnitureParticleSystemParticle particle in Particles)
        {
            particle.Dispose();
        }

        Particles = null!;
        _particleConfigs = null!;
        base.Dispose();
    }

    protected override void Ignite()
    {
        HasIgnited = true;

        if (_totalSpawned < _maxParticles && Age > 1)
        {
            ReleaseParticles();
        }
    }

    public override void Update()
    {
        base.Update();
        Verlet();

        if (!IsAlive && _totalSpawned < _maxParticles)
        {
            if (Age % _burstPulse == 0)
            {
                ReleaseParticles();
            }
        }
    }

    public void Setup(
        int maxParticles,
        int particlesPerFrame,
        double energy,
        double dirX,
        double dirY,
        double dirZ,
        double gravity,
        double airFriction,
        string shape,
        double initialSpeed,
        int fuseTime,
        int burstPulse)
    {
        _maxParticles = maxParticles;
        _particlesPerFrame = particlesPerFrame;
        _energy = energy;

        double len = Math.Sqrt((dirX * dirX) + (dirY * dirY) + (dirZ * dirZ));

        if (len > 0)
        {
            _directionX = dirX / len;
            _directionY = dirY / len;
            _directionZ = dirZ / len;
        }

        _gravity = gravity;
        _airFriction = airFriction;
        _shape = shape;
        _fuseTime = fuseTime;
        _initialSpeed = initialSpeed;
        _burstPulse = burstPulse;
        Reset();
    }

    public void Reset()
    {
        foreach (FurnitureParticleSystemParticle p in Particles)
        {
            p.Dispose();
        }

        Particles = new List<FurnitureParticleSystemParticle>();
        _totalSpawned = 0;
        HasIgnited = false;
        Init(0, 0, 0, _directionX, _directionY, _directionZ, _energy, _timeStep, _fuseTime, true);
    }

    public void CopyStateFrom(FurnitureParticleSystemEmitter other, double scaleFactor)
    {
        Copy(other, scaleFactor);
        _energy = other._energy;
        _directionX = other._directionX;
        _directionY = other._directionY;
        _directionZ = other._directionZ;
        _gravity = other._gravity;
        _airFriction = other._airFriction;
        _shape = other._shape;
        _fuseTime = other._fuseTime;
        _initialSpeed = other._initialSpeed;
        _burstPulse = other._burstPulse;
        _timeStep = other._timeStep;
        HasIgnited = other.HasIgnited;
    }

    public void ConfigureParticle(int lifeTime, bool isEmitter, IGraphicAsset[]? frames, bool fade)
    {
        _particleConfigs.Add(new ParticleConfig(lifeTime, isEmitter, frames, fade));
    }

    public void Verlet()
    {
        double dt2 = _timeStep * _timeStep;

        if (IsAlive || _totalSpawned < _maxParticles)
        {
            double prevX = X;
            double prevY = Y;
            double prevZ = Z;

            X = ((2 - _airFriction) * X) - ((1 - _airFriction) * LastX);
            Y = ((2 - _airFriction) * Y) - ((1 - _airFriction) * LastY) + (_gravity * dt2);
            Z = ((2 - _airFriction) * Z) - ((1 - _airFriction) * LastZ);

            LastX = prevX;
            LastY = prevY;
            LastZ = prevZ;
        }

        List<FurnitureParticleSystemParticle> dead = new();

        foreach (FurnitureParticleSystemParticle p in Particles)
        {
            p.Update();

            double prevX = p.X;
            double prevY = p.Y;
            double prevZ = p.Z;

            p.X = ((2 - _airFriction) * p.X) - ((1 - _airFriction) * p.LastX);
            p.Y = ((2 - _airFriction) * p.Y) - ((1 - _airFriction) * p.LastY) + (_gravity * dt2);
            p.Z = ((2 - _airFriction) * p.Z) - ((1 - _airFriction) * p.LastZ);

            p.LastX = prevX;
            p.LastY = prevY;
            p.LastZ = prevZ;

            if (p.Y > 10 || !p.IsAlive)
            {
                dead.Add(p);
            }
        }

        foreach (FurnitureParticleSystemParticle p in dead)
        {
            Particles.Remove(p);
            p.Dispose();
        }
    }

    private void ReleaseParticles()
    {
        for (int i = 0; i < _particlesPerFrame; i++)
        {
            double dirX = 0;
            double dirY = 0;
            double dirZ = 0;

            switch (_shape)
            {
                case SHAPE_CONE:
                    dirX = RandomSign() * Random.Shared.NextDouble();
                    dirY = -(Random.Shared.NextDouble() + 1);
                    dirZ = RandomSign() * Random.Shared.NextDouble();
                    break;

                case SHAPE_PLANE:
                    dirX = RandomSign() * Random.Shared.NextDouble();
                    dirY = 0;
                    dirZ = RandomSign() * Random.Shared.NextDouble();
                    break;

                case SHAPE_SPHERE:
                    dirX = RandomSign() * Random.Shared.NextDouble();
                    dirY = RandomSign() * Random.Shared.NextDouble();
                    dirZ = RandomSign() * Random.Shared.NextDouble();
                    break;
            }

            double len = Math.Sqrt((dirX * dirX) + (dirY * dirY) + (dirZ * dirZ));

            if (len > 0)
            {
                dirX /= len;
                dirY /= len;
                dirZ /= len;
            }

            ParticleConfig? config = GetRandomConfig();
            FurnitureParticleSystemParticle particle = new();

            int lifeTime;
            bool isEmitter;
            IGraphicAsset[]? frames;
            bool fade;

            if (config != null)
            {
                lifeTime = (int)Math.Floor((Random.Shared.NextDouble() * config.LifeTime) + 10);
                isEmitter = config.IsEmitter;
                frames = config.Frames;
                fade = config.Fade;
            }
            else
            {
                lifeTime = (int)Math.Floor((Random.Shared.NextDouble() * 20) + 10);
                isEmitter = false;
                frames = [];
                fade = false;
            }

            particle.Init(X, Y, Z, dirX, dirY, dirZ, _initialSpeed, _timeStep, lifeTime, isEmitter, frames, fade);
            Particles.Add(particle);
            _totalSpawned++;
        }
    }

    private ParticleConfig? GetRandomConfig()
    {
        if (_particleConfigs.Count == 0)
        {
            return null;
        }

        int index = (int)Math.Floor(Random.Shared.NextDouble() * _particleConfigs.Count);
        return _particleConfigs[index];
    }

    private static double RandomSign()
    {
        return Random.Shared.NextDouble() < 0.5 ? 1.0 : -1.0;
    }

    private sealed record ParticleConfig(int LifeTime, bool IsEmitter, IGraphicAsset[]? Frames, bool Fade);
}
