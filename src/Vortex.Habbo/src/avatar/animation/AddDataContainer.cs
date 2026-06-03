// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/AddDataContainer.as

using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Animation;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/AddDataContainer.as
public class AddDataContainer
{
    /// @see AddDataContainer.as::AddDataContainer
    public AddDataContainer(XElement xml)
    {
        Id = (string?)xml.Attribute("id") ?? "";
        Align = (string?)xml.Attribute("align") ?? "";
        Base = (string?)xml.Attribute("base") ?? "";
        Ink = (string?)xml.Attribute("ink") ?? "";

        string blendStr = (string?)xml.Attribute("blend") ?? "";

        if (blendStr.Length <= 0)
        {
            return;
        }

        Blend = double.TryParse(blendStr, out double b) ? b : 1;

        if (Blend > 1)
        {
            Blend /= 100;
        }
    }

    /// @see AddDataContainer.as::get id
    public string Id { get; }

    /// @see AddDataContainer.as::get align
    public string Align { get; }

    /// @see AddDataContainer.as::get base
    public string Base { get; }

    /// @see AddDataContainer.as::get ink
    public string Ink { get; }

    /// @see AddDataContainer.as::get blend
    public double Blend { get; } = 1;

    /// @see AddDataContainer.as::get isBlended
    public bool IsBlended => Blend != 1;
}
