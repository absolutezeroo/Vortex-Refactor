using System;

using Godot;

using Vortex.Room.Object;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureCuboidVisualization
public class FurnitureCuboidVisualization : RoomObjectSpriteVisualization
{
    private Dictionary<string, Image>? _assetCache;
    private List<FurniturePlane> _planes = [];
    private bool _planesInitialized;
    private int _updateCounter;

    public override void Dispose()
    {
        base.Dispose();

        if (_assetCache != null)
        {
            _assetCache.Clear();
            _assetCache = null;
        }

        foreach (FurniturePlane plane in _planes)
        {
            plane.Dispose();
        }

        _planes = null!;
    }

    public override bool Initialize(IRoomObjectVisualizationData? data)
    {
        Reset();
        return true;
    }

    public override void Update(IRoomGeometry? geometry, int time, bool fullUpdate, bool needsUpdate)
    {
        IRoomObject? obj = Object;

        if (obj == null)
        {
            return;
        }

        if (_assetCache == null)
        {
            _assetCache = new Dictionary<string, Image>();
        }

        if (geometry == null)
        {
            return;
        }

        InitializePlanes();
        UpdatePlanes(geometry, time);
    }

    protected virtual void DefineSprites()
    {
        CreateSprites(1);
    }

    protected virtual void InitializePlanes()
    {
        if (_planesInitialized)
        {
            return;
        }

        IRoomObject? obj = Object;

        if (obj == null)
        {
            return;
        }

        double sizeX = obj.Model.GetNumber("furniture_size_x");
        double sizeY = obj.Model.GetNumber("furniture_size_y");
        double sizeZ = obj.Model.GetNumber("furniture_size_z");

        if (double.IsNaN(sizeX) || double.IsNaN(sizeY) || double.IsNaN(sizeZ))
        {
            return;
        }

        Vector3d leftSide = new(sizeX, 0, 0);
        Vector3d rightSide = new(0, sizeY, 0);
        Vector3d origin = new(-0.5, -0.5, 0);

        FurniturePlane plane = new(origin, leftSide, rightSide);
        plane.Color = 0xFFFF00;
        _planes.Add(plane);
        _planesInitialized = true;
        DefineSprites();
    }

    protected virtual void UpdatePlanes(IRoomGeometry geometry, int time)
    {
        IRoomObject? obj = Object;

        if (obj == null || _assetCache == null)
        {
            return;
        }

        _updateCounter++;

        for (int i = 0; i < _planes.Count; i++)
        {
            bool updated = false;
            string assetName = "plane " + i + " " + geometry.Scale;

            FurniturePlane plane = _planes[i];
            int direction = (int)obj.Direction.X;

            if (direction / 45 == 2 || direction / 45 == 6)
            {
                plane.SetRotation(true);
            }
            else
            {
                plane.SetRotation(false);
            }

            if (plane.Update(geometry, time))
            {
                Image? planeBitmap = plane.BitmapData;

                if (planeBitmap != null)
                {
                    _assetCache[assetName] = planeBitmap;
                }
                else
                {
                    _assetCache.Remove(assetName);
                }

                updated = true;
            }

            IRoomObjectSprite? sprite = GetSprite(i);

            if (sprite != null)
            {
                Vector2 offset = plane.Offset;
                sprite.OffsetX = -(int)offset.X;
                sprite.OffsetY = -(int)offset.Y;
                sprite.Color = (int)plane.Color;
                sprite.Visible = plane.Visible;

                if (_assetCache.TryGetValue(assetName, out Image? cachedImage))
                {
                    sprite.Asset = cachedImage;
                }
                else
                {
                    sprite.Asset = null;
                }

                if (updated)
                {
                    sprite.AssetName = assetName + "_" + obj.Id + "_" + _updateCounter;
                }

                sprite.RelativeDepth = plane.RelativeDepth;
            }
        }
    }
}
