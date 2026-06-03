namespace Vortex.Room.Object;

/// @see com.sulake.room.object.IRoomObjectModelController
public interface IRoomObjectModelController : IRoomObjectModel
{
    void SetNumber(string key, double value, bool locked = false);
    void SetString(string key, string value, bool locked = false);
    void SetNumberArray(string key, List<int> value, bool locked = false);
    void SetStringArray(string key, string[] value, bool locked = false);
    void SetStringToStringMap(string key, Dictionary<string, string> value, bool locked = false);
}
