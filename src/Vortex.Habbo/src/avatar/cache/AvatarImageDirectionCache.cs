// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/cache/AvatarImageDirectionCache.as

namespace Vortex.Habbo.Avatar.Cache;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/cache/AvatarImageDirectionCache.as
public class AvatarImageDirectionCache
{
    private const string KEY_SEPARATOR = "/";
    private const string NO_FRAMES_KEY = "-";

    private List<AvatarImagePartContainer>? _partList;
    private Dictionary<string, AvatarImageBodyPartContainer?>? _cache;
    private Dictionary<int, string>? _keyCache;

    /// @see AvatarImageDirectionCache.as::AvatarImageDirectionCache
    public AvatarImageDirectionCache(List<AvatarImagePartContainer> partList)
    {
        _cache = new Dictionary<string, AvatarImageBodyPartContainer?>();
        _partList = partList;
        _keyCache = new Dictionary<int, string>();
    }

    /// @see AvatarImageDirectionCache.as::dispose
    public void Dispose()
    {
        if (_cache != null)
        {
            foreach (AvatarImageBodyPartContainer? container in _cache.Values)
            {
                container?.Dispose();
            }
            _cache = null;
        }
        _partList = null;
        _keyCache = null;
    }

    /// @see AvatarImageDirectionCache.as::getPartList
    public List<AvatarImagePartContainer>? GetPartList()
    {
        return _partList;
    }

    /// @see AvatarImageDirectionCache.as::getImageContainer
    public AvatarImageBodyPartContainer? GetImageContainer(int frame)
    {
        string key = GetCacheKey(frame);

        _cache!.TryGetValue(key, out AvatarImageBodyPartContainer? container);

        return container;
    }

    /// @see AvatarImageDirectionCache.as::updateImageContainer
    public void UpdateImageContainer(AvatarImageBodyPartContainer container, int frame)
    {
        string key = GetCacheKey(frame);

        if (_cache!.TryGetValue(key, out AvatarImageBodyPartContainer? existing) && existing != null)
        {
            existing.Dispose();
        }

        _cache[key] = container;
    }

    /// @see AvatarImageDirectionCache.as::getCacheKey
    private string GetCacheKey(int frame)
    {
        if (_partList == null || _partList.Count == 0)
        {
            return NO_FRAMES_KEY;
        }

        if (_keyCache!.TryGetValue(frame, out string? cached))
        {
            return cached;
        }

        string key = _partList[0].GetCacheableKey(frame);

        for (int i = 1;
             i < _partList.Count;
             i++)
        {
            key += KEY_SEPARATOR + _partList[i].GetCacheableKey(frame);
        }

        _keyCache[frame] = key;
        return key;
    }
}
