// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/alias/AssetAlias.as

using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Alias;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/alias/AssetAlias.as
public class AssetAlias
{
    /// @see AssetAlias.as::AssetAlias
    public AssetAlias(XElement xml)
    {
        Name = (string?)xml.Attribute("name") ?? "";
        Link = (string?)xml.Attribute("link") ?? "";
        FlipH = int.TryParse((string?)xml.Attribute("fliph"), out int fh) && fh != 0;
        FlipV = int.TryParse((string?)xml.Attribute("flipv"), out int fv) && fv != 0;
    }

    /// @see AssetAlias.as::get name
    public string Name { get; }

    /// @see AssetAlias.as::get link
    public string Link { get; }

    /// @see AssetAlias.as::get flipH
    public bool FlipH { get; }

    /// @see AssetAlias.as::get flipV
    public bool FlipV { get; }
}
