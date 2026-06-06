using System;

using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurniturePlanetSystemVisualizationPlanetObject
public class FurniturePlanetSystemVisualizationPlanetObject(string name,
    int index,
    double radius,
    double arcSpeed,
    double arcOffset,
    double height)
{
    private const double SYSTEM_TEMPO = 30;

    private readonly double _arcSpeed = arcSpeed * Math.PI * 2.0 / 360.0;
    private readonly double _arcOffset = arcOffset * Math.PI * 2.0 / 360.0;
    private double _angle;
    private readonly Vector3d _position = new(0, 0, 0);
    private readonly List<FurniturePlanetSystemVisualizationPlanetObject> _children = new();

    public string Name { get; } = name;

    public void Dispose()
    {
        while (_children.Count > 0)
        {
            FurniturePlanetSystemVisualizationPlanetObject child = _children[0];
            _children.RemoveAt(0);
            child.Dispose();
        }
    }

    public void Update(Vector3d[] offsetArray, Vector3d parentPosition, double scale)
    {
        _angle += _arcSpeed / SYSTEM_TEMPO;
        offsetArray[index] = GetPositionVector(parentPosition, scale);

        for (int i = 0; i < _children.Count; i++)
        {
            _children[i].Update(offsetArray, _position, scale);
        }
    }

    public Vector3d GetPositionVector(Vector3d? parent, double scale)
    {
        double cosVal = radius * Math.Cos(_angle + _arcOffset);
        double sinVal = radius * Math.Sin(_angle + _arcOffset);
        double halfScale = scale / 2.0;

        _position.X = (cosVal - sinVal) * halfScale;
        _position.Y = ((sinVal + cosVal) * halfScale * 0.5) - (height * halfScale);
        _position.Z = -(int)((4 * (cosVal + sinVal)) - 0.7);

        if (parent != null)
        {
            _position.Add(parent);
        }

        return _position;
    }

    public void AddChild(FurniturePlanetSystemVisualizationPlanetObject child)
    {
        if (!_children.Contains(child))
        {
            _children.Add(child);
        }
    }

    public bool HasChild(string name)
    {
        for (int i = 0; i < _children.Count; i++)
        {
            if (_children[i].Name == name)
            {
                return true;
            }

            if (_children[i].HasChild(name))
            {
                return true;
            }
        }

        return false;
    }

    public FurniturePlanetSystemVisualizationPlanetObject? GetChild(string name)
    {
        for (int i = 0; i < _children.Count; i++)
        {
            if (_children[i].Name == name)
            {
                return _children[i];
            }

            if (_children[i].HasChild(name))
            {
                return _children[i].GetChild(name);
            }
        }

        return null;
    }
}
