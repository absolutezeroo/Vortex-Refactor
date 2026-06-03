using Vortex.Room.Renderer.Utils;

namespace Vortex.Room.Renderer.Cache;

/// @see com.sulake.room.renderer.cache.BitmapDataCache (class_3807)
public class BitmapDataCache
{
    private Dictionary<string, BitmapDataCacheItem>? _items;
    private readonly int _memLimitMax;
    private readonly int _memLimitIncrement;

    public BitmapDataCache(int initialLimitMB, int maxLimitMB, int incrementMB = 1)
    {
        _items = new Dictionary<string, BitmapDataCacheItem>();
        MemLimit = initialLimitMB * 1024 * 1024;
        _memLimitMax = maxLimitMB * 1024 * 1024;
        _memLimitIncrement = incrementMB * 1024 * 1024;
        if (_memLimitIncrement < 0)
        {
            _memLimitIncrement = 0;
        }
    }

    public int MemUsage { get; private set; }

    public int MemLimit { get; private set; }

    public void Dispose()
    {
        if (_items != null)
        {
            List<string> keys = new(_items.Keys);
            foreach (string key in keys)
            {
                if (!RemoveItem(key))
                {
                    Logger.Warn($"Failed to remove item {key} from room canvas bitmap cache!");
                }
            }
            _items.Clear();
            _items = null;
        }
    }

    public void Compress()
    {
        if (MemUsage <= MemLimit)
        {
            return;
        }
        List<BitmapDataCacheItem> items = new(_items!.Values);
        items.Sort((a, b) => b.UseCount.CompareTo(a.UseCount));
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].UseCount > 1)
            {
                break;
            }
            RemoveItem(items[i].Name);
        }
        IncreaseMemoryLimit();
    }

    private void IncreaseMemoryLimit()
    {
        MemLimit += _memLimitIncrement;
        if (MemLimit > _memLimitMax)
        {
            MemLimit = _memLimitMax;
        }
    }

    private bool RemoveItem(string name)
    {
        if (name == null)
        {
            return false;
        }
        if (_items!.TryGetValue(name, out BitmapDataCacheItem? item))
        {
            if (item.UseCount > 1)
            {
                return false;
            }
            _items.Remove(name);
            MemUsage -= item.MemUsage;
            item.Dispose();
            return true;
        }
        return false;
    }

    public ExtendedBitmapData? GetBitmapData(string name)
    {
        if (_items!.TryGetValue(name, out BitmapDataCacheItem? item))
        {
            return item.BitmapData;
        }
        return null;
    }

    public void AddBitmapData(string name, ExtendedBitmapData data)
    {
        if (data == null || data.Width <= 0 || data.Height <= 0)
        {
            return;
        }
        if (_items!.TryGetValue(name, out BitmapDataCacheItem? existing))
        {
            if (existing.BitmapData != null)
            {
                MemUsage -= existing.BitmapData.Width * existing.BitmapData.Height * 4;
            }
            existing.BitmapData = data;
        }
        else
        {
            BitmapDataCacheItem item = new(data, name);
            _items[name] = item;
        }
        MemUsage += data.Width * data.Height * 4;
    }
}
