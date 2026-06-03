// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/cache/AvatarImageActionCache.as

using System.Diagnostics;

namespace Vortex.Habbo.Avatar.Cache;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/cache/AvatarImageActionCache.as
public class AvatarImageActionCache
{
    private static readonly Stopwatch Timer = Stopwatch.StartNew();

    private readonly Dictionary<string, AvatarImageDirectionCache> _cache;
    private long _lastAccessTimeMs;

    /// @see AvatarImageActionCache.as::AvatarImageActionCache
    public AvatarImageActionCache()
    {
        _cache = new Dictionary<string, AvatarImageDirectionCache>();
        SetLastAccessTime(Timer.ElapsedMilliseconds);
    }

    /// @see AvatarImageActionCache.as::dispose
    public void Dispose()
    {
        foreach (AvatarImageDirectionCache dirCache in _cache.Values)
        {
            dirCache.Dispose();
        }
        _cache.Clear();
    }

    /// @see AvatarImageActionCache.as::getDirectionCache
    public AvatarImageDirectionCache? GetDirectionCache(int direction)
    {
        _cache.TryGetValue(direction.ToString(), out AvatarImageDirectionCache? result);

        return result;
    }

    /// @see AvatarImageActionCache.as::updateDirectionCache
    public void UpdateDirectionCache(int direction, AvatarImageDirectionCache cache)
    {
        _cache[direction.ToString()] = cache;
    }

    /// @see AvatarImageActionCache.as::setLastAccessTime
    public void SetLastAccessTime(long timeMs)
    {
        _lastAccessTimeMs = timeMs;
    }

    /// @see AvatarImageActionCache.as::getLastAccessTime
    public long GetLastAccessTime()
    {
        return _lastAccessTimeMs;
    }
}
