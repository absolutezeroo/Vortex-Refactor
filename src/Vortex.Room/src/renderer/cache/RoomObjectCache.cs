using Vortex.Room.Data;
using Vortex.Room.Object.Enum;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Renderer.Utils;

namespace Vortex.Room.Renderer.Cache;

/// @see com.sulake.room.renderer.cache.RoomObjectCache (class_3826)
public class RoomObjectCache(string depthKey)
{
    private const int MAX_SIZE_FOR_AVG_COLOR = 200;

    private Dictionary<string, RoomObjectCacheItem>? _objects = new();

    public void Dispose()
    {
        if (_objects != null)
        {
            foreach (RoomObjectCacheItem item in _objects.Values)
            {
                item.Dispose();
            }
            _objects.Clear();
            _objects = null;
        }
    }

    public RoomObjectCacheItem GetObjectCache(string key)
    {
        if (_objects!.TryGetValue(key, out RoomObjectCacheItem? item))
        {
            return item;
        }
        item = new RoomObjectCacheItem(depthKey);
        _objects[key] = item;
        return item;
    }

    public void RemoveObjectCache(string key)
    {
        if (_objects!.Remove(key, out RoomObjectCacheItem? item))
        {
            item.Dispose();
        }
    }

    public List<RoomObjectSpriteData> GetSortableSpriteList()
    {
        List<RoomObjectSpriteData> result = new();
        if (_objects == null)
        {
            return result;
        }

        foreach (RoomObjectCacheItem cacheItem in _objects.Values)
        {
            if (cacheItem.Sprites == null)
            {
                continue;
            }
            foreach (SortableSprite sortable in cacheItem.Sprites.Sprites)
            {
                IRoomObjectSprite? sprite = sortable.Sprite;
                if (sprite == null)
                {
                    continue;
                }
                if (sprite.SpriteType == RoomObjectSpriteType.ROOM_PLANE)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(sprite.LibraryAssetName))
                {
                    continue;
                }

                RoomObjectSpriteData data = new()
                {
                    ObjectId = cacheItem.ObjectId,
                    X = sortable.X,
                    Y = sortable.Y,
                    Z = sortable.Z,
                    Name = sprite.LibraryAssetName ?? "",
                    FlipH = sprite.FlipH,
                    Alpha = sprite.Alpha,
                    Color = sprite.Color.ToString(),
                    BlendMode = sprite.BlendMode ?? "normal",
                    Width = sprite.Width,
                    Height = sprite.Height,
                    ObjectType = sprite.ObjectType,
                    Posture = sprite.AssetPosture,
                };

                if (IsSkewedSprite(sprite))
                {
                    data.Skew = sprite.Direction % 4 == 0 ? -0.5 : 0.5;
                }

                result.Add(data);
            }
        }

        return result;
    }

    private static bool IsSkewedSprite(Object.Visualization.IRoomObjectSprite sprite)
    {
        if (sprite.ObjectType == null)
        {
            return false;
        }
        if (sprite.ObjectType.StartsWith("external_image_wallitem") && sprite.Tag == "THUMBNAIL")
        {
            return true;
        }
        if (sprite.ObjectType.StartsWith("guild_forum") && sprite.Tag == "THUMBNAIL")
        {
            return true;
        }
        return false;
    }

    public List<SortableSprite> GetPlaneSortableSprites()
    {
        List<SortableSprite> result = new();
        if (_objects == null)
        {
            return result;
        }

        foreach (RoomObjectCacheItem cacheItem in _objects.Values)
        {
            if (cacheItem.Sprites == null)
            {
                continue;
            }
            foreach (SortableSprite sortable in cacheItem.Sprites.Sprites)
            {
                if (sortable.Sprite?.SpriteType == RoomObjectSpriteType.ROOM_PLANE)
                {
                    result.Add(sortable);
                }
            }
        }

        return result;
    }
}
