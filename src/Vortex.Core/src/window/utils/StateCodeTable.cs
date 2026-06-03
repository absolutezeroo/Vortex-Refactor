// @see core/window/utils/class_3578.as

namespace Vortex.Core.Window.Utils;

/// <summary>
/// Static window state code table mapping state names to bit flag values.
/// </summary>
/// @see core/window/utils/class_3578.as
public static class StateCodeTable
{
    /// @see core/window/utils/class_3578.as::fillTables
    public static void FillTables(Dictionary<string, uint> nameToCode, Dictionary<uint, string>? codeToName = null)
    {
        nameToCode["default"] = 0;
        nameToCode["active"] = 1;
        nameToCode["focused"] = 2;
        nameToCode["hovering"] = 4;
        nameToCode["selected"] = 8;
        nameToCode["pressed"] = 16;
        nameToCode["disabled"] = 32;
        nameToCode["locked"] = 64;

        if (codeToName == null)
        {
            return;
        }

        foreach (KeyValuePair<string, uint> kvp in nameToCode)
        {
            codeToName[kvp.Value] = kvp.Key;
        }
    }
}
