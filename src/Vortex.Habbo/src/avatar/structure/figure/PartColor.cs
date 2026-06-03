// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/PartColor.as

using System;
using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Structure.Figure;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/PartColor.as
public class PartColor : IPartColor
{
    /// @see PartColor.as::PartColor
    public PartColor(XElement xml)
    {
        Id = int.Parse(xml.Attribute("id")?.Value ?? "0");
        Index = int.Parse(xml.Attribute("index")?.Value ?? "0");
        ClubLevel = int.Parse(xml.Attribute("club")?.Value ?? "0");

        string selectableStr = xml.Attribute("selectable")?.Value ?? "0";
        IsSelectable = int.TryParse(selectableStr, out int selVal) && selVal != 0;

        string hexStr = xml.Value;
        Rgb = Convert.ToUInt32(hexStr, 16);
        R = (Rgb >> 16) & 0xFF;
        G = (Rgb >> 8) & 0xFF;
        B = (Rgb >> 0) & 0xFF;
        RedMultiplier = R / 255.0 * 1.0;
        GreenMultiplier = G / 255.0 * 1.0;
        BlueMultiplier = B / 255.0 * 1.0;
    }

    /// @see PartColor.as::get id
    public int Id { get; }

    /// @see PartColor.as::get index
    public int Index { get; }

    /// @see PartColor.as::get clubLevel
    public int ClubLevel { get; }

    /// @see PartColor.as::get isSelectable
    public bool IsSelectable { get; }

    /// @see PartColor.as::get rgb
    public uint Rgb { get; }

    /// @see PartColor.as::get r
    public uint R { get; }

    /// @see PartColor.as::get g
    public uint G { get; }

    /// @see PartColor.as::get b
    public uint B { get; }

    /// @see PartColor.as::get redMultiplier
    public double RedMultiplier { get; }

    /// @see PartColor.as::get greenMultiplier
    public double GreenMultiplier { get; }

    /// @see PartColor.as::get blueMultiplier
    public double BlueMultiplier { get; }
}
