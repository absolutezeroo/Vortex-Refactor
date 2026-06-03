using System;

using Vortex.Room.Object;
using Vortex.Room.Utils;

namespace Vortex.Room.Renderer.Cache;

/// @see com.sulake.room.renderer.cache.RoomObjectLocationCacheItem (class_3686)
public class RoomObjectLocationCacheItem(string depthKey)
{
    private int _geometryUpdateId = -1;
    private int _objectUpdateId = -1;
    private readonly Vector3d _cachedLocation = new();
    private Vector3d? _screenLocation = new();

    public bool LocationChanged { get; private set; }

    public void Dispose()
    {
        _screenLocation = null;
    }

    public IVector3d? GetScreenLocation(IRoomObject obj, IRoomGeometry geometry)
    {
        if (obj == null || geometry == null)
        {
            return null;
        }

        bool needsUpdate = false;
        IVector3d location = obj.Location;

        if (geometry.UpdateId != _geometryUpdateId || obj.UpdateId != _objectUpdateId)
        {
            _objectUpdateId = obj.UpdateId;
            if (geometry.UpdateId != _geometryUpdateId ||
                location.X != _cachedLocation.X ||
                location.Y != _cachedLocation.Y ||
                location.Z != _cachedLocation.Z)
            {
                _geometryUpdateId = geometry.UpdateId;
                _cachedLocation.Assign(location);
                needsUpdate = true;
            }
        }

        LocationChanged = needsUpdate;

        if (needsUpdate)
        {
            IVector3d? screenPos = geometry.GetScreenPosition(location);
            if (screenPos == null)
            {
                return null;
            }

            double depth = obj.Model.GetNumber(depthKey);
            if (double.IsNaN(depth) || depth == 0)
            {
                Vector3d rounded = new(Math.Round(location.X), Math.Round(location.Y), location.Z);
                if (rounded.X != location.X || rounded.Y != location.Y)
                {
                    IVector3d? roundedScreen = geometry.GetScreenPosition(rounded);
                    _screenLocation!.Assign(screenPos);
                    if (roundedScreen != null)
                    {
                        _screenLocation.Z = roundedScreen.Z;
                    }
                }
                else
                {
                    _screenLocation!.Assign(screenPos);
                }
            }
            else
            {
                _screenLocation!.Assign(screenPos);
            }

            _screenLocation!.X = Math.Round(_screenLocation.X);
            _screenLocation.Y = Math.Round(_screenLocation.Y);
        }

        return _screenLocation;
    }
}
