using System.Xml.Linq;

using Vortex.Bundle.Data;
using Vortex.Bundle.Converter.Convert;

namespace Vortex.Bundle.Converter.Extract;

/// <summary>
/// Parses animation.xml from a Habbo SWF into AnimationData[].
/// Used for figure part animations (avatars), not furniture visualization animations.
/// @see nitro-converter-main/src/common/mapping/mappers/asset/AnimationMapper.ts
/// </summary>
public static class AnimationXmlParser
{
    public static AnimationData[] Parse(XDocument doc, StringTableBuilder strings)
    {
        List<AnimationData> result = new();
        XElement? root = doc.Root;
        if (root == null)
        {
            return [];
        }

        XElement animRoot = root.Name.LocalName == "animationSet"
            ? root
            : root.Element("animationSet") ?? root;

        foreach (XElement animEl in animRoot.Elements("animation"))
        {
            string? name = (string?)animEl.Attribute("name");
            if (name == null)
            {
                continue;
            }

            AnimationData anim = new()
            {
                NameIndex = strings.Add(name),
                DescriptionIndex = OptString(animEl, "desc", strings),
                ResetOnToggle = ParseBool(animEl.Attribute("resetOnToggle")),
            };

            anim.Sprites = ParseSprites(animEl, strings);
            anim.Frames = ParseFrames(animEl, strings);
            anim.Overrides = ParseOverrides(animEl, strings);
            anim.Adds = ParseAdds(animEl, strings);
            anim.Removes = ParseRemoves(animEl, strings);
            anim.Shadows = ParseShadows(animEl, strings);
            anim.Avatars = ParseAvatars(animEl, strings);

            result.Add(anim);
        }

        return result.ToArray();
    }

    private static AnimSprite[] ParseSprites(XElement animEl, StringTableBuilder strings)
    {
        List<AnimSprite> sprites = new();
        XElement? spritesEl = animEl.Element("sprites");
        if (spritesEl == null)
        {
            return [];
        }

        foreach (XElement sEl in spritesEl.Elements("sprite"))
        {
            sprites.Add(new AnimSprite
            {
                IdIndex = OptString(sEl, "id", strings),
                MemberIndex = OptString(sEl, "member", strings),
                Directions = (byte)((int?)sEl.Attribute("directions") ?? 0),
                StaticY = ParseInt16(sEl.Attribute("staticY")),
                InkIndex = OptString(sEl, "ink", strings),
            });
        }

        return sprites.ToArray();
    }

    private static AnimFrame[] ParseFrames(XElement animEl, StringTableBuilder strings)
    {
        List<AnimFrame> frames = new();
        XElement? framesEl = animEl.Element("frames");
        if (framesEl == null)
        {
            return [];
        }

        foreach (XElement fEl in framesEl.Elements("frame"))
        {
            AnimFrame frame = new()
            {
                Number = ParseUInt16(fEl.Attribute("number")),
            };

            List<AnimFramePart> parts = new();
            foreach (XElement pEl in fEl.Elements("bodypart"))
            {
                parts.Add(new AnimFramePart
                {
                    SetTypeIndex = OptString(pEl, "setType", strings),
                    SetIdIndex = OptString(pEl, "id", strings),
                    ActionIndex = OptString(pEl, "action", strings),
                    Dx = ParseInt16(pEl.Attribute("dx")),
                    Dy = ParseInt16(pEl.Attribute("dy")),
                    Dd = ParseInt16(pEl.Attribute("dd")),
                });
            }

            // Also check <fx> parts
            foreach (XElement pEl in fEl.Elements("fx"))
            {
                parts.Add(new AnimFramePart
                {
                    SetTypeIndex = OptString(pEl, "setType", strings),
                    SetIdIndex = OptString(pEl, "id", strings),
                    ActionIndex = OptString(pEl, "action", strings),
                    Dx = ParseInt16(pEl.Attribute("dx")),
                    Dy = ParseInt16(pEl.Attribute("dy")),
                    Dd = ParseInt16(pEl.Attribute("dd")),
                });
            }

            frame.Parts = parts.ToArray();
            frames.Add(frame);
        }

        return frames.ToArray();
    }

    private static AnimOverride[] ParseOverrides(XElement animEl, StringTableBuilder strings)
    {
        List<AnimOverride> overrides = new();
        XElement? overridesEl = animEl.Element("overrides");
        if (overridesEl == null)
        {
            return [];
        }

        foreach (XElement oEl in overridesEl.Elements("override"))
        {
            AnimOverride over = new()
            {
                NameIndex = OptString(oEl, "name", strings), OverrideIndex = OptString(oEl, "override", strings),
            };

            List<AnimFrame> frames = new();
            foreach (XElement fEl in oEl.Elements("frame"))
            {
                AnimFrame frame = new()
                {
                    Number = ParseUInt16(fEl.Attribute("number")),
                };
                List<AnimFramePart> parts = new();
                foreach (XElement pEl in fEl.Elements("bodypart"))
                {
                    parts.Add(new AnimFramePart
                    {
                        SetTypeIndex = OptString(pEl, "setType", strings),
                        SetIdIndex = OptString(pEl, "id", strings),
                        ActionIndex = OptString(pEl, "action", strings),
                        Dx = ParseInt16(pEl.Attribute("dx")),
                        Dy = ParseInt16(pEl.Attribute("dy")),
                        Dd = ParseInt16(pEl.Attribute("dd")),
                    });
                }
                frame.Parts = parts.ToArray();
                frames.Add(frame);
            }

            over.Frames = frames.ToArray();
            overrides.Add(over);
        }

        return overrides.ToArray();
    }

    private static AnimAdd[] ParseAdds(XElement animEl, StringTableBuilder strings)
    {
        List<AnimAdd> adds = new();
        XElement? addsEl = animEl.Element("adds");
        if (addsEl == null)
        {
            return [];
        }

        foreach (XElement aEl in addsEl.Elements("add"))
        {
            adds.Add(new AnimAdd
            {
                IdIndex = OptString(aEl, "id", strings),
                AlignIndex = OptString(aEl, "align", strings),
                BaseIndex = OptString(aEl, "base", strings),
            });
        }

        return adds.ToArray();
    }

    private static AnimRemove[] ParseRemoves(XElement animEl, StringTableBuilder strings)
    {
        List<AnimRemove> removes = new();
        XElement? removesEl = animEl.Element("removes");
        if (removesEl == null)
        {
            return [];
        }

        foreach (XElement rEl in removesEl.Elements("remove"))
        {
            removes.Add(new AnimRemove
            {
                IdIndex = OptString(rEl, "id", strings),
            });
        }

        return removes.ToArray();
    }

    private static AnimShadow[] ParseShadows(XElement animEl, StringTableBuilder strings)
    {
        List<AnimShadow> shadows = new();
        XElement? shadowsEl = animEl.Element("shadows");
        if (shadowsEl == null)
        {
            return [];
        }

        foreach (XElement sEl in shadowsEl.Elements("shadow"))
        {
            shadows.Add(new AnimShadow
            {
                IdIndex = OptString(sEl, "id", strings),
            });
        }

        return shadows.ToArray();
    }

    private static AnimAvatar[] ParseAvatars(XElement animEl, StringTableBuilder strings)
    {
        List<AnimAvatar> avatars = new();
        XElement? avatarsEl = animEl.Element("avatars");
        if (avatarsEl == null)
        {
            return [];
        }

        foreach (XElement aEl in avatarsEl.Elements("avatar"))
        {
            avatars.Add(new AnimAvatar
            {
                InkIndex = OptString(aEl, "ink", strings),
                ForegroundIndex = OptString(aEl, "foreground", strings),
                BackgroundIndex = OptString(aEl, "background", strings),
            });
        }

        return avatars.ToArray();
    }

    private static uint OptString(XElement el, string attrName, StringTableBuilder strings)
    {
        string? val = (string?)el.Attribute(attrName);
        return val != null ? strings.Add(val) : VortexBundleFormat.NULL_STRING;
    }

    private static ushort ParseUInt16(XAttribute? attr)
    {
        return attr != null && ushort.TryParse(attr.Value, out ushort v) ? v : (ushort)0;
    }

    private static short ParseInt16(XAttribute? attr)
    {
        return attr != null && short.TryParse(attr.Value, out short v) ? v : (short)0;
    }

    private static bool ParseBool(XAttribute? attr)
    {
        if (attr == null)
        {
            return false;
        }
        string val = attr.Value.ToLowerInvariant();
        return val is "1" or "true";
    }
}
