using Vortex.Room.Object;

namespace Vortex.Room;

/// @see com.sulake.room.RoomObjectManager
public class RoomObjectManager : IRoomObjectManager
{
    private Dictionary<string, IRoomObjectController>? _objects = new();
    private Dictionary<string, Dictionary<string, IRoomObjectController>>? _typeIndex = new();

    public int ObjectCount => _objects?.Count ?? 0;

    public void Dispose()
    {
        Reset();
        _objects = null;
        _typeIndex = null;
    }

    public IRoomObjectController? CreateObject(int id, uint stateCount, string type)
    {
        RoomObject obj = new(id, (int)stateCount, type);
        return AddObject(id.ToString(), type, obj);
    }

    private IRoomObjectController? AddObject(string key, string type, IRoomObjectController obj)
    {
        if (_objects!.ContainsKey(key))
        {
            obj.Dispose();

            return null;
        }

        _objects[key] = obj;
        Dictionary<string, IRoomObjectController>? typeMap = GetObjectsForType(type, true)!;
        typeMap[key] = obj;

        return obj;
    }

    private Dictionary<string, IRoomObjectController>? GetObjectsForType(string type, bool create = true)
    {
        if (_typeIndex!.TryGetValue(type, out Dictionary<string, IRoomObjectController>? map))
        {
            return map;
        }

        if (!create)
        {
            return null;
        }

        map = new Dictionary<string, IRoomObjectController>();
        _typeIndex[type] = map;

        return map;
    }

    public IRoomObjectController? GetObject(int id)
    {
        if (_objects != null && _objects.TryGetValue(id.ToString(), out IRoomObjectController? obj))
        {
            return obj;
        }

        return null;
    }

    public List<IRoomObjectController>? GetObjects()
    {
        if (_objects == null)
        {
            return null;
        }

        return [.. _objects.Values];
    }

    public IRoomObjectController? GetObjectWithIndex(int index)
    {
        if (_objects == null || index < 0 || index >= _objects.Count)
        {
            return null;
        }
        int i = 0;
        foreach (KeyValuePair<string, IRoomObjectController> kvp in _objects)
        {
            if (i == index)
            {
                return kvp.Value;
            }
            i++;
        }
        return null;
    }

    public int GetObjectCountForType(string type)
    {
        Dictionary<string, IRoomObjectController>? map = GetObjectsForType(type, false);
        return map?.Count ?? 0;
    }

    public IRoomObjectController? GetObjectWithIndexAndType(int index, string type)
    {
        Dictionary<string, IRoomObjectController>? map = GetObjectsForType(type, false);
        if (map == null || index < 0 || index >= map.Count)
        {
            return null;
        }
        int i = 0;
        foreach (KeyValuePair<string, IRoomObjectController> kvp in map)
        {
            if (i == index)
            {
                return kvp.Value;
            }
            i++;
        }
        return null;
    }

    public bool DisposeObject(int id)
    {
        string key = id.ToString();
        if (_objects != null && _objects.Remove(key, out IRoomObjectController? obj))
        {
            string type = obj.Type;
            Dictionary<string, IRoomObjectController>? map = GetObjectsForType(type, false);
            map?.Remove(key);
            obj.Dispose();
            return true;
        }
        return false;
    }

    public void Reset()
    {
        if (_objects != null)
        {
            foreach (IRoomObjectController obj in _objects.Values)
            {
                obj.Dispose();
            }
            _objects.Clear();
        }
        _typeIndex?.Clear();
    }
}
