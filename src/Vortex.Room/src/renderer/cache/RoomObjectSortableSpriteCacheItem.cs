using Vortex.Room.Renderer.Utils;

namespace Vortex.Room.Renderer.Cache;

/// @see com.sulake.room.renderer.cache.RoomObjectSortableSpriteCacheItem (class_3726)
public class RoomObjectSortableSpriteCacheItem
{
    private int _updateId1 = -1;
    private int _updateId2 = -1;

    public int SpriteCount => Sprites.Count;
    public bool IsEmpty { get; private set; }

    public List<SortableSprite> Sprites { get; } =
        [];

    public void Dispose()
    {
        SetSpriteCount(0);
    }

    public void AddSprite(SortableSprite sprite)
    {
        Sprites.Add(sprite);
    }

    public SortableSprite GetSprite(int index)
    {
        return Sprites[index];
    }

    public bool NeedsUpdate(int updateId1, int updateId2)
    {
        if (updateId1 != _updateId1 || updateId2 != _updateId2)
        {
            _updateId1 = updateId1;
            _updateId2 = updateId2;
            return true;
        }
        return false;
    }

    public void SetSpriteCount(int count)
    {
        if (count < Sprites.Count)
        {
            for (int i = count; i < Sprites.Count; i++)
            {
                Sprites[i].Dispose();
            }
            Sprites.RemoveRange(count, Sprites.Count - count);
        }
        IsEmpty = Sprites.Count == 0;
    }
}
