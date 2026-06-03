// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/geometry/AvatarSet.as

using System.Linq;
using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Geometry;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/geometry/AvatarSet.as
public class AvatarSet
{
    private readonly Dictionary<string, AvatarSet> _subSets;
    private readonly List<string> _bodyPartIds;
    private readonly string[] _allBodyPartIds;
    private readonly bool _isMain;

    /// @see AvatarSet.as::AvatarSet
    public AvatarSet(XElement xml)
    {
        Id = xml.Attribute("id")?.Value ?? "";

        // AS3: _isMain = _loc3_ == null ? false : Boolean(parseInt(_loc3_))
        string? mainAttr = xml.Attribute("main")?.Value;
        _isMain = mainAttr != null && int.TryParse(mainAttr, out int mainVal) && mainVal != 0;

        _subSets = new Dictionary<string, AvatarSet>();
        _bodyPartIds = new List<string>();

        foreach (XElement subSetXml in xml.Elements("avatarset"))
        {
            AvatarSet subSet = new(subSetXml);
            _subSets[subSetXml.Attribute("id")?.Value ?? ""] = subSet;
        }

        foreach (XElement bodyPartXml in xml.Elements("bodypart"))
        {
            _bodyPartIds.Add(bodyPartXml.Attribute("id")?.Value ?? "");
        }

        // _allBodyPartIds = own ids + all children's getBodyParts() flattened
        List<string> allParts = new(_bodyPartIds);

        foreach (AvatarSet subSet in _subSets.Values)
        {
            allParts.AddRange(subSet.GetBodyParts());
        }

        _allBodyPartIds = allParts.ToArray();
    }

    /// @see AvatarSet.as::findAvatarSet
    public AvatarSet? FindAvatarSet(string id)
    {
        if (id == Id)
        {
            return this;
        }

        foreach (AvatarSet subSet in _subSets.Values)
        {
            if (subSet.FindAvatarSet(id) != null)
            {
                return subSet;
            }
        }

        return null;
    }

    /// @see AvatarSet.as::getBodyParts
    public string[] GetBodyParts()
    {
        // AS3: return var_4622.concat() — returns a copy
        return (string[])_allBodyPartIds.Clone();
    }

    /// @see AvatarSet.as::get id
    public string Id { get; }

    /// @see AvatarSet.as::get isMain
    public bool IsMain => _isMain || _subSets.Values.Any(subSet => subSet.IsMain);
}
