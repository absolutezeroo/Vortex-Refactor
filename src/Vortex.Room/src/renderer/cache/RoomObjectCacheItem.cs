namespace Vortex.Room.Renderer.Cache;

/// @see com.sulake.room.renderer.cache.RoomObjectCacheItem (class_3730)
public class RoomObjectCacheItem(string depthKey)
{
    public int ObjectId { get; set; }
    public RoomObjectLocationCacheItem? Location { get; private set; } = new(depthKey);

    public RoomObjectSortableSpriteCacheItem? Sprites { get; private set; } = new();

    public void Dispose()
    {
        if (Location != null)
        {
            Location.Dispose();
            Location = null;
        }
        if (Sprites != null)
        {
            Sprites.Dispose();
            Sprites = null;
        }
    }
}
