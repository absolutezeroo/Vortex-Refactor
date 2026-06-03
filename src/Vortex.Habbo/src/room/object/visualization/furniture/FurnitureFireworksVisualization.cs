using System.Globalization;
using System.Xml.Linq;

using Vortex.Room.Object;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureFireworksVisualization
public class FurnitureFireworksVisualization : AnimatedFurnitureVisualization
{
    private Dictionary<int, FurnitureParticleSystem>? _particleSystems;
    private FurnitureParticleSystem? _activeParticleSystem;

    public override void Dispose()
    {
        base.Dispose();
        _activeParticleSystem = null;

        if (_particleSystems == null)
        {
            return;
        }

        foreach (FurnitureParticleSystem system in _particleSystems.Values)
        {
            system.Dispose();
        }

        _particleSystems = null;
    }

    protected override bool UpdateObject(double scale, double geometryDirection)
    {
        if (!base.UpdateObject(scale, geometryDirection))
        {
            return false;
        }

        if (_particleSystems == null)
        {
            ReadDefinition();

            if (_particleSystems != null)
            {
                _particleSystems.TryGetValue((int)scale, out _activeParticleSystem);
            }
            else
            {
                Logger.Warn("ERROR Particle systems could not be read! " + Object?.Type);
            }
        }
        else
        {
            _particleSystems.TryGetValue((int)scale, out FurnitureParticleSystem? newSystem);

            if (scale == _previousScale && newSystem == _activeParticleSystem)
            {
                return true;
            }

            if (newSystem != null)
            {
                newSystem.CopyStateFrom(_activeParticleSystem);
            }

            _activeParticleSystem?.Reset();
            _activeParticleSystem = newSystem;
        }

        return true;

    }

    protected override void UpdateSprites(int scale, bool fullUpdate, int layerMask)
    {
        base.UpdateSprites(scale, fullUpdate, layerMask);

        _activeParticleSystem?.UpdateSprites();
    }

    protected override int UpdateAnimation(double scale)
    {
        _activeParticleSystem?.UpdateAnimation();

        return base.UpdateAnimation(scale);
    }

    protected override void SetAnimation(int animationId)
    {
        _activeParticleSystem?.SetAnimation(animationId);

        base.SetAnimation(animationId);
    }

    protected override int GetSpriteYOffset(int scale, int direction, int layer)
    {
        if (_activeParticleSystem != null && _activeParticleSystem.ControlsSprite(layer))
        {
            return _activeParticleSystem.GetSpriteYOffset(scale, direction, layer);
        }

        return base.GetSpriteYOffset(scale, direction, layer);
    }

    private void ReadDefinition()
    {
        IRoomObject? obj = Object;

        if (obj == null)
        {
            return;
        }

        IRoomObjectModel model = obj.Model;
        string? xmlData = model.GetString("furniture_fireworks_data");

        if (string.IsNullOrEmpty(xmlData))
        {
            Logger.Warn("No Particle System Data Found.");

            return;
        }

        XElement root = XElement.Parse(xmlData);
        _particleSystems = new Dictionary<int, FurnitureParticleSystem>();

        foreach (XElement psElement in root.Elements("particlesystem"))
        {
            XAttribute? sizeAttr = psElement.Attribute("size");

            if (sizeAttr == null)
            {
                Logger.Warn("Particle System does not have size property!");

                continue;
            }

            int size = int.Parse(sizeAttr.Value, CultureInfo.InvariantCulture);
            FurnitureParticleSystem system = new(this);

            system.ParseData(psElement);

            _particleSystems[size] = system;
        }
    }
}
