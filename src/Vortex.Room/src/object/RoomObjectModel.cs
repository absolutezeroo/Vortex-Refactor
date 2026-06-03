namespace Vortex.Room.Object;

/// @see com.sulake.room.object.RoomObjectModel
public class RoomObjectModel : IRoomObjectModelController
{
    private const string MAP_KEYS_PREFIX = "ROMC_MAP_KEYS_";
    private const string MAP_VALUES_PREFIX = "ROMC_MAP_VALUES_";

    private Dictionary<string, double>? _numbers = new();
    private Dictionary<string, string>? _strings = new();
    private Dictionary<string, List<int>>? _numberArrays = new();
    private Dictionary<string, string[]>? _stringArrays = new();

    private List<string> _lockedNumbers =
        [];
    private List<string> _lockedStrings =
        [];
    private List<string> _lockedNumberArrays =
        [];
    private List<string> _lockedStringArrays =
        [];

    public int UpdateId { get; private set; } = 0;

    public void Dispose()
    {
        _numbers?.Clear();
        _numbers = null;
        _strings?.Clear();
        _strings = null;
        _numberArrays?.Clear();
        _numberArrays = null;
        _stringArrays?.Clear();
        _stringArrays = null;
        _lockedNumbers = [];
        _lockedStrings = [];
        _lockedNumberArrays = [];
        _lockedStringArrays = [];
    }

    public bool HasNumber(string key)
    {
        return _numbers != null && _numbers.ContainsKey(key);
    }

    public bool HasNumberArray(string key)
    {
        return _numberArrays != null && _numberArrays.ContainsKey(key);
    }

    public bool HasString(string key)
    {
        return _strings != null && _strings.ContainsKey(key);
    }

    public bool HasStringArray(string key)
    {
        return _stringArrays != null && _stringArrays.ContainsKey(key);
    }

    public double GetNumber(string key)
    {
        if (_numbers != null && _numbers.TryGetValue(key, out double value))
        {
            return value;
        }
        return double.NaN;
    }

    public string? GetString(string key)
    {
        if (_strings != null && _strings.TryGetValue(key, out string? value))
        {
            return value;
        }
        return null;
    }

    public List<int>? GetNumberArray(string key)
    {
        if (_numberArrays != null && _numberArrays.TryGetValue(key, out List<int>? value))
        {
            return new List<int>(value);
        }
        return null;
    }

    public string[]? GetStringArray(string key)
    {
        if (_stringArrays != null && _stringArrays.TryGetValue(key, out string[]? value))
        {
            return (string[])value.Clone();
        }
        return null;
    }

    public Dictionary<string, string>? GetStringToStringMap(string key)
    {
        string[]? keys = GetStringArray(MAP_KEYS_PREFIX + key);
        string[]? values = GetStringArray(MAP_VALUES_PREFIX + key);
        if (keys == null || values == null || keys.Length != values.Length)
        {
            return new Dictionary<string, string>();
        }
        Dictionary<string, string> map = new();
        for (int i = 0; i < keys.Length; i++)
        {
            map[keys[i]] = values[i];
        }
        return map;
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
        if (_numbers!.TryGetValue(key, out double existing) && existing == value)
        {
            return;
        }
        _numbers[key] = value;
        UpdateId++;
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
        if (_strings!.TryGetValue(key, out string? existing) && existing == value)
        {
            return;
        }
        _strings[key] = value;
        UpdateId++;
    }

    public void SetNumberArray(string key, List<int> value, bool locked = false)
    {
        if (value == null)
        {
            return;
        }
        if (_lockedNumberArrays.Contains(key))
        {
            return;
        }
        if (locked)
        {
            _lockedNumberArrays.Add(key);
        }

        List<int> filtered = new(value);

        if (_numberArrays!.TryGetValue(key, out List<int>? existing) && existing.Count == filtered.Count)
        {
            bool equal = true;
            for (int i = filtered.Count - 1; i >= 0; i--)
            {
                if (filtered[i] != existing[i])
                {
                    equal = false;
                    break;
                }
            }
            if (equal)
            {
                return;
            }
        }

        _numberArrays[key] = filtered;
        UpdateId++;
    }

    public void SetStringArray(string key, string[] value, bool locked = false)
    {
        if (value == null)
        {
            return;
        }
        if (_lockedStringArrays.Contains(key))
        {
            return;
        }
        if (locked)
        {
            _lockedStringArrays.Add(key);
        }

        string[] filtered = (string[])value.Clone();

        if (_stringArrays!.TryGetValue(key, out string[]? existing) && existing.Length == filtered.Length)
        {
            bool equal = true;
            for (int i = filtered.Length - 1; i >= 0; i--)
            {
                if (filtered[i] != existing[i])
                {
                    equal = false;
                    break;
                }
            }
            if (equal)
            {
                return;
            }
        }

        _stringArrays[key] = filtered;
        UpdateId++;
    }

    public void SetStringToStringMap(string key, Dictionary<string, string> value, bool locked = false)
    {
        if (value == null)
        {
            return;
        }
        string[] keys = new string[value.Count];
        string[] values = new string[value.Count];
        int i = 0;
        foreach (KeyValuePair<string, string> kvp in value)
        {
            keys[i] = kvp.Key;
            values[i] = kvp.Value;
            i++;
        }
        SetStringArray(MAP_KEYS_PREFIX + key, keys, locked);
        SetStringArray(MAP_VALUES_PREFIX + key, values, locked);
    }
}
