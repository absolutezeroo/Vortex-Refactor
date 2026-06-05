using System;

namespace Vortex.Habbo.Toolbar;

/// @see com.sulake.habbo.toolbar.HabboToolbarIconEnum
public static class HabboToolbarIconEnum
{
    public const int ICON_HELP = 0;
    public const int ICON_NAVIGATOR = 1;
    public const int ICON_CATALOGUE = 2;
    public const int ICON_INVENTORY = 3;
    public const int ICON_QUESTS = 4;
    public const int ICON_ACHIEVEMENTS = 5;
    public const int ICON_MEMENU = 6;
    public const int ICON_GAMES = 7;
    public const int ICON_STORIES = 8;
    public const int ICON_RECEPTION = 9;
    public const int ICON_HOME = 10;
    public const int ICON_GUIDE = 11;
    public const int ICON_BUILDER = 12;
    public const int ICON_CAMERA = 13;
    public const int ICON_WIRED_MENU = 14;
    public const int ICON_ROOMINFO = 15;
    public const int ICON_GROUP = 16;

    private static readonly Dictionary<int, string> _TOOLBAR_NAMES = new()
    {
        { ICON_HELP,         "HELP"         },
        { ICON_NAVIGATOR,    "NAVIGATOR"    },
        { ICON_CATALOGUE,    "CATALOGUE"    },
        { ICON_INVENTORY,    "INVENTORY"    },
        { ICON_QUESTS,       "QUESTS"       },
        { ICON_ACHIEVEMENTS, "ACHIEVEMENTS" },
        { ICON_MEMENU,       "MEMENU"       },
        { ICON_GAMES,        "GAMES"        },
        { ICON_STORIES,      "STORIES"      },
        { ICON_RECEPTION,    "RECEPTION"    },
        { ICON_HOME,         "HOME"         },
        { ICON_GUIDE,        "GUIDE"        },
        { ICON_BUILDER,      "BUILDER"      },
        { ICON_CAMERA,       "CAMERA"       },
        { ICON_WIRED_MENU,   "WIRED_MENU"   },
        { ICON_ROOMINFO,     "ROOMINFO"     },
        { ICON_GROUP,        "GROUP"        },
    };

    /// @see HabboToolbarIconEnum.as::_TOOLBAR_NAMES — reverse lookup by name
    public static int GetIconId(string iconName)
    {
        if (string.Equals(iconName, "me", StringComparison.OrdinalIgnoreCase))
        {
            return ICON_MEMENU;
        }

        foreach (KeyValuePair<int, string> pair in _TOOLBAR_NAMES)
        {
            if (string.Equals(pair.Value, iconName, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Key;
            }
        }
        return -1;
    }

    public static string? GetIconName(int iconId)
    {
        return _TOOLBAR_NAMES.TryGetValue(iconId, out string? name) ? name : null;
    }
}
