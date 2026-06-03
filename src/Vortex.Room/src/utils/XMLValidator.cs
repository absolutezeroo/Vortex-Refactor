using System.Linq;
using System.Xml.Linq;

namespace Vortex.Room.Utils;

/// <summary>
/// Validates that XML elements contain all required attributes.
/// </summary>
/// @see com.sulake.room.utils.class_1781
public static class XMLValidator
{
    public static bool CheckRequiredAttributes(XElement? element, string[] attributes)
    {
        if (element == null || attributes == null)
        {
            return false;
        }

        return attributes.All(t => element.Attribute(t) != null);
    }
}
