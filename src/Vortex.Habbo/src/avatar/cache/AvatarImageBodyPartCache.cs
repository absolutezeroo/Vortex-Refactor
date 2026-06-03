// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/cache/AvatarImageBodyPartCache.as

using Vortex.Habbo.Avatar.Actions;

namespace Vortex.Habbo.Avatar.Cache;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/cache/AvatarImageBodyPartCache.as
public class AvatarImageBodyPartCache
{
    private Dictionary<string, AvatarImageActionCache>? _cache;
    private IActiveActionData? _currentAction;
    private int _direction;
    private bool _disposed;

    /// @see AvatarImageBodyPartCache.as::AvatarImageBodyPartCache
    public AvatarImageBodyPartCache()
    {
        _cache = new Dictionary<string, AvatarImageActionCache>();
    }

    /// @see AvatarImageBodyPartCache.as::setAction
    public void SetAction(IActiveActionData action, long timeMs)
    {
        _currentAction ??= action;

        AvatarImageActionCache? actionCache = GetActionCache(_currentAction);
        actionCache?.SetLastAccessTime(timeMs);

        _currentAction = action;
    }

    /// @see AvatarImageBodyPartCache.as::dispose
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_cache != null)
        {
            DisposeActions(0, long.MaxValue);
            _cache = null;
        }

        _disposed = true;
    }

    /// @see AvatarImageBodyPartCache.as::disposeActions
    public void DisposeActions(long maxAgeMs, long currentTimeMs)
    {
        if (_cache == null || _disposed)
        {
            return;
        }

        List<string> keysToRemove = new();

        foreach ((string key, AvatarImageActionCache actionCache) in _cache)
        {
            long lastAccess = actionCache.GetLastAccessTime();

            if (currentTimeMs - lastAccess < maxAgeMs)
            {
                continue;
            }

            actionCache.Dispose();
            keysToRemove.Add(key);
        }

        foreach (string key in keysToRemove)
        {
            _cache.Remove(key);
        }
    }

    /// @see AvatarImageBodyPartCache.as::getAction
    public IActiveActionData? GetAction()
    {
        return _currentAction;
    }

    /// @see AvatarImageBodyPartCache.as::setDirection
    public void SetDirection(int direction)
    {
        _direction = direction;
    }

    /// @see AvatarImageBodyPartCache.as::getDirection
    public int GetDirection()
    {
        return _direction;
    }

    /// @see AvatarImageBodyPartCache.as::getActionCache
    public AvatarImageActionCache? GetActionCache(IActiveActionData? action = null)
    {
        if (_currentAction == null)
        {
            return null;
        }

        action ??= _currentAction;

        string key = action.OverridingAction ?? action.Id;
        _cache!.TryGetValue(key, out AvatarImageActionCache? result);
        return result;
    }

    /// @see AvatarImageBodyPartCache.as::updateActionCache
    public void UpdateActionCache(IActiveActionData action, AvatarImageActionCache cache)
    {
        string key = action.OverridingAction ?? action.Id;
        _cache![key] = cache;
    }
}
