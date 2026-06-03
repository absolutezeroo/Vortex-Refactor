// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/AvatarCanvas.as

using System.Xml.Linq;

using Godot;

namespace Vortex.Habbo.Avatar.Structure;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/AvatarCanvas.as
public class AvatarCanvas
{
    /// @see AvatarCanvas.as::AvatarCanvas
    public AvatarCanvas(XElement xml, string scale)
    {
        Id = xml.Attribute("id")?.Value ?? "";
        Width = int.Parse(xml.Attribute("width")?.Value ?? "0");
        Height = int.Parse(xml.Attribute("height")?.Value ?? "0");
        Offset = new Vector2I(
            int.Parse(xml.Attribute("dx")?.Value ?? "0"),
            int.Parse(xml.Attribute("dy")?.Value ?? "0")
        );

        if (scale == "h")
        {
            RegPoint = new Vector2I((Width - 64) / 2, 0);
        }
        else
        {
            RegPoint = new Vector2I((Width - 32) / 2, 0);
        }
    }

    /// @see AvatarCanvas.as::get width
    public int Width { get; }

    /// @see AvatarCanvas.as::get height
    public int Height { get; }

    /// @see AvatarCanvas.as::get offset
    public Vector2I Offset { get; }

    /// @see AvatarCanvas.as::get id
    public string Id { get; }

    /// @see AvatarCanvas.as::get regPoint
    public Vector2I RegPoint { get; }
}
