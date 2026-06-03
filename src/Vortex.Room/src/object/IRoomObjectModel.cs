namespace Vortex.Room.Object;

/// @see com.sulake.room.object.IRoomObjectModel
public interface IRoomObjectModel
{
    bool HasNumber(string key);
    bool HasNumberArray(string key);
    bool HasString(string key);
    bool HasStringArray(string key);
    double GetNumber(string key);
    string? GetString(string key);
    List<int>? GetNumberArray(string key);
    string[]? GetStringArray(string key);
    int UpdateId { get; }
    Dictionary<string, string>? GetStringToStringMap(string key);
}
