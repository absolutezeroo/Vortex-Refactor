using System.Globalization;
using System.Xml.Linq;

using Vortex.Bundle.Data;
using Vortex.Bundle.Converter.Convert;

namespace Vortex.Bundle.Converter.Extract;

/// <summary>
/// Parses logic.xml from a Habbo SWF into LogicData.
/// @see nitro-converter-main/src/common/mapping/mappers/asset/LogicMapper.ts
/// </summary>
public static class LogicXmlParser
{
    public static LogicData Parse(XDocument doc, StringTableBuilder strings)
    {
        LogicData logic = new();
        XElement? root = doc.Root;
        if (root == null)
        {
            return logic;
        }

        XElement logicRoot = root.Name.LocalName == "objectData"
            ? root
            : root.Element("objectData") ?? root;

        // Model: dimensions and directions
        XElement? modelEl = logicRoot.Element("model");
        if (modelEl != null)
        {
            logic.HasModel = true;
            XElement? dimsEl = modelEl.Element("dimensions");
            if (dimsEl != null)
            {
                logic.DimensionX = ParseInt16(dimsEl.Attribute("x"), 1);
                logic.DimensionY = ParseInt16(dimsEl.Attribute("y"), 1);
                logic.DimensionZ = ParseFloat(dimsEl.Attribute("z"), 1.0f);
                logic.CenterZ = ParseFloat(dimsEl.Attribute("centerZ"), 0.0f);
            }

            XElement? dirsEl = modelEl.Element("directions");
            if (dirsEl != null)
            {
                List<ushort> dirs = new();
                foreach (XElement dirEl in dirsEl.Elements("direction"))
                {
                    dirs.Add(ParseUInt16(dirEl.Attribute("id")));
                }
                logic.Directions = dirs.ToArray();
            }
        }

        // Action
        XElement? actionEl = logicRoot.Element("action");
        if (actionEl != null)
        {
            logic.HasAction = true;
            string? link = (string?)actionEl.Attribute("link");
            logic.ActionLinkIndex = link != null ? strings.Add(link) : VortexBundleFormat.NULL_STRING;
            logic.ActionStartState = (int?)actionEl.Attribute("startState") ?? 0;
        }

        // Sound (credits)
        XElement? creditsEl = logicRoot.Element("credits");
        if (creditsEl != null)
        {
            logic.HasSound = true;
            logic.SoundSampleId = ParseUInt32(creditsEl.Attribute("sampleId"));
            string? soundName = (string?)creditsEl.Attribute("name");
            logic.SoundNameIndex = soundName != null ? strings.Add(soundName) : VortexBundleFormat.NULL_STRING;
        }

        // Particle systems
        XElement? particlesEl = logicRoot.Element("particleSystems");
        if (particlesEl != null)
        {
            logic.HasParticles = true;
            List<byte[]> blobs = new();
            foreach (XElement pEl in particlesEl.Elements("particleSystem"))
            {
                // Store raw XML as blob for now
                blobs.Add(System.Text.Encoding.UTF8.GetBytes(pEl.ToString()));
            }
            logic.ParticleBlobs = blobs.ToArray();
        }

        return logic;
    }

    private static ushort ParseUInt16(XAttribute? attr, ushort def = 0)
    {
        return attr != null && ushort.TryParse(attr.Value, out ushort v) ? v : def;
    }

    private static short ParseInt16(XAttribute? attr, short def = 0)
    {
        return attr != null && short.TryParse(attr.Value, out short v) ? v : def;
    }

    private static uint ParseUInt32(XAttribute? attr, uint def = 0)
    {
        return attr != null && uint.TryParse(attr.Value, out uint v) ? v : def;
    }

    private static float ParseFloat(XAttribute? attr, float def = 0)
    {
        return attr != null && float.TryParse(attr.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : def;
    }
}
