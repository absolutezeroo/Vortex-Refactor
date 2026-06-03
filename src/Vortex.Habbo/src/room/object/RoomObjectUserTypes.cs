using System.Linq;

namespace Vortex.Habbo.Room.Object;

/// <summary>
/// User type string constants and type ID mapping for room user objects.
/// </summary>
/// @see com.sulake.habbo.room.object.RoomObjectUserTypes
public static class RoomObjectUserTypes
{
    public const string USER = "user";
    public const string PET = "pet";
    public const string BOT = "bot";
    public const string RENTABLE_BOT = "rentable_bot";
    public const string MONSTERPLANT = "monsterplant";

    private static readonly Dictionary<string, int> _ids = new()
    {
        {
            USER, 1
        },
        {
            PET, 2
        },
        {
            BOT, 3
        },
        {
            RENTABLE_BOT, 4
        },
    };

    public static int GetTypeId(string name)
    {
        return _ids.GetValueOrDefault(name, 0);
    }

    public static string? GetName(int typeId)
    {
        return (from kvp in _ids where kvp.Value == typeId select kvp.Key).FirstOrDefault();
    }

    public static string GetVisualizationType(string name)
    {
        if (name is BOT or RENTABLE_BOT)
        {
            return USER;
        }

        return name;
    }
}
