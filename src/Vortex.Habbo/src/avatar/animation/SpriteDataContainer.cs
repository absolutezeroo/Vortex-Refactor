// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/SpriteDataContainer.as

using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Animation;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/SpriteDataContainer.as
public class SpriteDataContainer : ISpriteDataContainer
{
    private readonly int[] _dx;
    private readonly int[] _dy;
    private readonly int[] _dz;

    /// @see SpriteDataContainer.as::SpriteDataContainer
    public SpriteDataContainer(IAnimation animation, XElement xml)
    {
        Animation = animation;
        Id = (string?)xml.Attribute("id") ?? "";
        Ink = (int?)xml.Attribute("ink") ?? 0;
        Member = (string?)xml.Attribute("member") ?? "";
        HasStaticY = ((int?)xml.Attribute("staticY") ?? 0) != 0;
        HasDirections = ((int?)xml.Attribute("directions") ?? 0) != 0;

        // Pre-allocate for 8 directions max
        _dx = new int[8];
        _dy = new int[8];
        _dz = new int[8];

        foreach (XElement dirEl in xml.Elements("direction"))
        {
            int dirId = (int?)dirEl.Attribute("id") ?? 0;
            if (dirId is >= 0 and < 8)
            {
                _dx[dirId] = (int?)dirEl.Attribute("dx") ?? 0;
                _dy[dirId] = (int?)dirEl.Attribute("dy") ?? 0;
                _dz[dirId] = (int?)dirEl.Attribute("dz") ?? 0;
            }
        }
    }

    /// @see SpriteDataContainer.as::getDirectionOffsetX
    public int GetDirectionOffsetX(int direction)
    {
        if (direction >= 0 && direction < _dx.Length)
        {
            return _dx[direction];
        }
        return 0;
    }

    /// @see SpriteDataContainer.as::getDirectionOffsetY
    public int GetDirectionOffsetY(int direction)
    {
        if (direction >= 0 && direction < _dy.Length)
        {
            return _dy[direction];
        }
        return 0;
    }

    /// @see SpriteDataContainer.as::getDirectionOffsetZ
    public int GetDirectionOffsetZ(int direction)
    {
        if (direction >= 0 && direction < _dz.Length)
        {
            return _dz[direction];
        }
        return 0;
    }

    /// @see SpriteDataContainer.as::get animation
    public IAnimation Animation { get; }

    /// @see SpriteDataContainer.as::get id
    public string Id { get; }

    /// @see SpriteDataContainer.as::get ink
    public int Ink { get; }

    /// @see SpriteDataContainer.as::get member
    public string Member { get; }

    /// @see SpriteDataContainer.as::get hasDirections
    public bool HasDirections { get; }

    /// @see SpriteDataContainer.as::get hasStaticY
    public bool HasStaticY { get; }
}
