using Vortex.Room.Object;
using Vortex.Room.Object.Logic;
using Vortex.Room.Renderer;

namespace Vortex.Room;

/// @see com.sulake.room.RoomInstance
public class RoomInstance(string id, IRoomInstanceContainer container) : IRoomInstance
{
    private Dictionary<string, double>? _numbers = new();
    private Dictionary<string, string>? _strings = new();
    private List<string> _lockedNumbers = [];
    private List<string> _lockedStrings = [];
    private Dictionary<string, IRoomObjectManager>? _managers = new();
    private List<int>? _updateCategories = [];
    private IRoomRendererBase? _renderer;
    private IRoomInstanceContainer? _container = container;

    public string Id => id;

    public void Dispose()
    {
        if (_managers != null)
        {
            foreach (IRoomObjectManager manager in _managers.Values)
            {
                manager.Dispose();
            }

            _managers.Clear();
            _managers = null;
        }

        if (_renderer != null)
        {
            _renderer.Dispose();
            _renderer = null;
        }

        _container = null;
        _updateCategories = null;

        _numbers?.Clear();
        _numbers = null;
        _strings?.Clear();
        _strings = null;
        _lockedNumbers = [];
        _lockedStrings = [];
    }

    public double GetNumber(string key)
    {
        if (_numbers != null && _numbers.TryGetValue(key, out double value))
        {
            return value;
        }

        return double.NaN;
    }

    public void SetNumber(string key, double value, bool locked = false)
    {
        if (_lockedNumbers.Contains(key))
        {
            return;
        }

        if (locked)
        {
            _lockedNumbers.Add(key);
        }

        if (!_numbers!.TryGetValue(key, out double existing) || existing != value)
        {
            _numbers[key] = value;
        }
    }

    public string? GetString(string key)
    {
        if (_strings != null && _strings.TryGetValue(key, out string? value))
        {
            return value;
        }

        return null;
    }

    public void SetString(string key, string value, bool locked = false)
    {
        if (_lockedStrings.Contains(key))
        {
            return;
        }

        if (locked)
        {
            _lockedStrings.Add(key);
        }

        if (!_strings!.TryGetValue(key, out string? existing) || existing != value)
        {
            _strings[key] = value;
        }
    }

    public void AddObjectUpdateCategory(int category)
    {
        if (_updateCategories!.Contains(category))
        {
            return;
        }

        _updateCategories.Add(category);
    }

    public void RemoveObjectUpdateCategory(int category)
    {
        _updateCategories!.Remove(category);
    }

    public void Update()
    {
        int time = System.Environment.TickCount;

        for (int i = _updateCategories!.Count - 1;
             i >= 0;
             i--)
        {
            int category = _updateCategories[i];
            IRoomObjectManager? manager = GetObjectManager(category);

            if (manager == null)
            {
                continue;
            }

            for (int j = manager.ObjectCount - 1;
                 j >= 0;
                 j--)
            {
                IRoomObjectController? obj = manager.GetObjectWithIndex(j);

                if (obj == null)
                {
                    continue;
                }

                IRoomObjectEventHandler? handler = obj.EventHandler;

                handler?.Update(time);
            }
        }
    }

    public IRoomObject? CreateRoomObject(int id1, string type, int stateCount)
    {
        return _container?.CreateRoomObject(id, id1, type, stateCount);
    }

    public IRoomObject? CreateObjectInternal(int id, int stateCount, string type, int category)
    {
        IRoomObjectManager? manager = CreateObjectManager(category);

        if (manager == null)
        {
            return null;
        }

        IRoomObjectController? obj = manager.CreateObject(id, (uint)stateCount, type);

        if (obj != null)
        {
            _renderer?.FeedRoomObject(obj);
        }

        return obj;
    }

    public IRoomObject? GetObject(int id, int category)
    {
        IRoomObjectManager? manager = GetObjectManager(category);

        return manager?.GetObject(id);
    }

    public List<IRoomObject>? GetObjects(int category)
    {
        IRoomObjectManager? manager = GetObjectManager(category);

        if (manager == null)
        {
            return [];
        }

        List<IRoomObjectController>? controllers = manager.GetObjects();

        if (controllers == null)
        {
            return [];
        }

        List<IRoomObject> result = new(controllers.Count);

        result.AddRange(controllers);

        return result;
    }

    public IRoomObject? GetObjectWithIndex(int index, int category)
    {
        IRoomObjectManager? manager = GetObjectManager(category);

        return manager?.GetObjectWithIndex(index);
    }

    public int GetObjectCount(int category)
    {
        IRoomObjectManager? manager = GetObjectManager(category);

        return manager?.ObjectCount ?? 0;
    }

    public IRoomObject? GetObjectWithIndexAndType(int index, string type, int category)
    {
        IRoomObjectManager? manager = GetObjectManager(category);

        return manager?.GetObjectWithIndexAndType(index, type);
    }

    public int GetObjectCountForType(string type, int category)
    {
        IRoomObjectManager? manager = GetObjectManager(category);

        return manager?.GetObjectCountForType(type) ?? 0;
    }

    public bool DisposeObject(int id, int category)
    {
        IRoomObjectManager? manager = GetObjectManager(category);

        if (manager == null)
        {
            return false;
        }

        IRoomObjectController? obj = manager.GetObject(id);

        if (obj == null)
        {
            return false;
        }

        obj.TearDown();
        _renderer?.RemoveRoomObject(obj);

        return manager.DisposeObject(id);
    }

    public int DisposeObjects(int category)
    {
        IRoomObjectManager? manager = GetObjectManager(category);
        if (manager == null)

        {
            return 0;
        }

        int count = manager.ObjectCount;

        for (int i = 0;
             i < count;
             i++)
        {
            IRoomObjectController? obj = manager.GetObjectWithIndex(i);

            if (obj == null)
            {
                continue;
            }

            _renderer?.RemoveRoomObject(obj);
            obj.Dispose();
        }
        manager.Reset();
        return count;
    }

    public void SetRenderer(IRoomRendererBase renderer)
    {
        if (renderer == _renderer)
        {
            return;
        }

        _renderer?.Dispose();
        _renderer = renderer;

        if (_renderer == null)
        {
            return;
        }

        _renderer.Reset();

        int[] managerIds = GetObjectManagerIds();

        for (int i = managerIds.Length - 1;
             i >= 0;
             i--)
        {
            int category = managerIds[i];
            int objCount = GetObjectCount(category);

            for (int j = objCount - 1;
                 j >= 0;
                 j--)
            {
                if (GetObjectWithIndex(j, category) is IRoomObjectController controller)
                {
                    _renderer.FeedRoomObject(controller);
                }
            }
        }
    }

    public IRoomRendererBase? GetRenderer()
    {
        return _renderer;
    }

    public int[] GetObjectManagerIds()
    {
        if (_managers == null)
        {
            return [];
        }

        int[] ids = new int[_managers.Count];
        int i = 0;

        foreach (string key in _managers.Keys)
        {
            if (int.TryParse(key, out int id))
            {
                ids[i++] = id;
            }
        }

        return ids[..i];
    }

    protected IRoomObjectManager? CreateObjectManager(int category)
    {
        string key = category.ToString();

        if (_managers!.TryGetValue(key, out IRoomObjectManager? existing))
        {
            return existing;
        }

        if (_container == null)
        {
            return null;
        }

        IRoomObjectManager? manager = _container.CreateRoomObjectManager();

        if (manager != null)
        {
            _managers[key] = manager;
        }

        return manager;
    }

    protected IRoomObjectManager? GetObjectManager(int category)
    {
        if (_managers != null && _managers.TryGetValue(category.ToString(), out IRoomObjectManager? manager))
        {
            return manager;
        }

        return null;
    }

    protected bool DisposeObjectManager(int category)
    {
        string key = category.ToString();
        DisposeObjects(category);

        if (_managers == null || !_managers.Remove(key, out IRoomObjectManager? manager))
        {
            return false;
        }

        manager.Dispose();

        return true;
    }

    public bool HasUninitializedObjects()
    {
        if (_managers == null)
        {
            return false;
        }

        foreach (IRoomObjectManager manager in _managers.Values)
        {
            int count = manager.ObjectCount;

            for (int i = 0;
                 i < count;
                 i++)
            {
                IRoomObjectController? obj = manager.GetObjectWithIndex(i);

                if (obj is
                    {
                        IsInitialized: false,
                    })
                {
                    return true;
                }
            }
        }

        return false;
    }
}
