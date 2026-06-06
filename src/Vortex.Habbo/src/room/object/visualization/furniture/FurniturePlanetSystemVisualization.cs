using System.Globalization;
using System.Xml.Linq;

using Vortex.Room.Object;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurniturePlanetSystemVisualization
public class FurniturePlanetSystemVisualization : AnimatedFurnitureVisualization
{
    private List<FurniturePlanetSystemVisualizationPlanetObject>? _planets;
    private List<string>? _planetNames;
    private Vector3d[] _offsetArray = [];
    private readonly Vector3d _originPosition = new(0, 0, 0);

    public override void Dispose()
    {
        if (_planets != null)
        {
            while (_planets.Count > 0)
            {
                FurniturePlanetSystemVisualizationPlanetObject planet = _planets[0];
                _planets.RemoveAt(0);
                planet.Dispose();
            }
        }

        _planets = null;
        _planetNames = null;
        base.Dispose();
    }

    protected override int UpdateAnimation(double scale)
    {
        if (_planets == null && SpriteCount > 0)
        {
            if (!ReadDefinition())
            {
                return 0;
            }
        }

        if (_planets != null)
        {
            for (int i = 0; i < _planets.Count; i++)
            {
                _planets[i].Update(_offsetArray, _originPosition, scale);
            }

            return base.UpdateAnimation(scale);
        }

        return 0;
    }

    protected override int GetSpriteXOffset(int scale, int direction, int layer)
    {
        if (layer < _offsetArray.Length)
        {
            return (int)_offsetArray[layer].X;
        }

        return base.GetSpriteXOffset(scale, direction, layer);
    }

    protected override int GetSpriteYOffset(int scale, int direction, int layer)
    {
        if (layer < _offsetArray.Length)
        {
            return (int)_offsetArray[layer].Y;
        }

        return base.GetSpriteYOffset(scale, direction, layer);
    }

    protected override double GetSpriteZOffset(int scale, int direction, int layer)
    {
        if (layer < _offsetArray.Length)
        {
            return _offsetArray[layer].Z;
        }

        return base.GetSpriteZOffset(scale, direction, layer);
    }

    private bool ReadDefinition()
    {
        IRoomObject? obj = Object;

        if (obj == null)
        {
            return false;
        }

        IRoomObjectModel model = obj.Model;
        string? xmlData = model.GetString("furniture_planetsystem_data");

        if (string.IsNullOrEmpty(xmlData))
        {
            return false;
        }

        XElement root = XElement.Parse(xmlData);
        IEnumerable<XElement> children = root.Elements();

        _planets = new List<FurniturePlanetSystemVisualizationPlanetObject>();
        _planetNames = new List<string>();

        int index = 0;
        int maxIndex = SpriteCount;
        _offsetArray = new Vector3d[maxIndex];

        for (int i = 0; i < maxIndex; i++)
        {
            _offsetArray[i] = new Vector3d();
        }

        foreach (XElement child in children)
        {
            if (GetSprite(index) != null)
            {
                string name = child.Attribute("name")?.Value ?? "";
                string parent = child.Attribute("parent")?.Value ?? "";
                double radius = double.Parse(child.Attribute("radius")?.Value ?? "0", CultureInfo.InvariantCulture);
                double arcSpeed = double.Parse(child.Attribute("arcspeed")?.Value ?? "0", CultureInfo.InvariantCulture);
                double arcOffset = double.Parse(child.Attribute("arcoffset")?.Value ?? "0", CultureInfo.InvariantCulture);
                double height = double.Parse(child.Attribute("height")?.Value ?? "0", CultureInfo.InvariantCulture);

                AddPlanet(name, index, parent, radius, arcSpeed, arcOffset, height);
            }

            index++;
        }

        return true;
    }

    private void AddPlanet(
        string name,
        int index,
        string parentName,
        double radius,
        double arcSpeed,
        double arcOffset,
        double height)
    {
        if (_planets == null)
        {
            return;
        }

        FurniturePlanetSystemVisualizationPlanetObject planet = new(name, index, radius, arcSpeed, arcOffset, height);
        FurniturePlanetSystemVisualizationPlanetObject? parent = GetPlanet(parentName);

        if (parent != null)
        {
            parent.AddChild(planet);
        }
        else
        {
            _planets.Add(planet);
            _planetNames!.Add(name);
        }
    }

    private FurniturePlanetSystemVisualizationPlanetObject? GetPlanet(string name)
    {
        if (_planets == null || string.IsNullOrEmpty(name))
        {
            return null;
        }

        for (int i = 0; i < _planets.Count; i++)
        {
            if (_planets[i].Name == name)
            {
                return _planets[i];
            }

            if (_planets[i].HasChild(name))
            {
                return _planets[i].GetChild(name);
            }
        }

        return null;
    }
}
