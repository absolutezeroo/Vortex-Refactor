using Vortex.Room.Renderer.Utils;

namespace Vortex.Room.Renderer.Cache;

/// @see com.sulake.room.renderer.cache.BitmapDataCacheItem (class_3846)
public class BitmapDataCacheItem
{
    private ExtendedBitmapData? _bitmapData;

    public string Name { get; }

    public BitmapDataCacheItem(ExtendedBitmapData? bitmapData, string name)
    {
        Name = name;
        _bitmapData = bitmapData;
        if (bitmapData != null)
        {
            bitmapData.AddReference();
            MemUsage = bitmapData.Width * bitmapData.Height * 4;
        }
    }

    public ExtendedBitmapData? BitmapData
    {
        get => _bitmapData;
        set
        {
            _bitmapData?.Dispose();
            _bitmapData = value;
            if (_bitmapData != null)
            {
                _bitmapData.AddReference();
                MemUsage = _bitmapData.Width * _bitmapData.Height * 4;
            }
            else
            {
                MemUsage = 0;
            }
        }
    }

    public int MemUsage { get; private set; }

    public int UseCount => _bitmapData?.ReferenceCount ?? 0;

    public void Dispose()
    {
        _bitmapData?.Dispose();
        _bitmapData = null;
        MemUsage = 0;
    }
}
