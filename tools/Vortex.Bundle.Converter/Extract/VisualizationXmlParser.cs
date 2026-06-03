using System.Globalization;
using System.Xml.Linq;

using Vortex.Bundle.Data;
using Vortex.Bundle.Converter.Convert;

namespace Vortex.Bundle.Converter.Extract;

/// <summary>
/// Parses visualization.xml from a Habbo SWF into VisualizationData[].
/// @see nitro-converter-main/src/common/mapping/mappers/asset/VisualizationMapper.ts
/// </summary>
public static class VisualizationXmlParser
{
    public static VisualizationData[] Parse(XDocument doc, StringTableBuilder strings)
    {
        List<VisualizationData> result = new();
        XElement? root = doc.Root;
        if (root == null)
        {
            return [];
        }

        XElement vizRoot = root.Name.LocalName == "visualizationData"
            ? root
            : root.Element("visualizationData") ?? root;

        foreach (XElement vizEl in vizRoot.Elements("visualization"))
        {
            int size = (int?)vizEl.Attribute("size") ?? 64;

            // Skip 32px visualizations (small icons)
            if (size == 32)
            {
                continue;
            }

            VisualizationData viz = new()
            {
                Size = (ushort)size,
                LayerCount = (ushort)((int?)vizEl.Attribute("layerCount") ?? 0),
                Angle = (ushort)((int?)vizEl.Attribute("angle") ?? 0),
            };

            viz.Layers = ParseLayers(vizEl, strings);
            viz.Colors = ParseColors(vizEl);
            viz.Directions = ParseDirections(vizEl);
            viz.Animations = ParseAnimations(vizEl);
            viz.Postures = ParsePostures(vizEl, strings);
            viz.Gestures = ParseGestures(vizEl, strings);

            result.Add(viz);
        }

        return result.ToArray();
    }

    private static VisualizationLayer[] ParseLayers(XElement vizEl, StringTableBuilder strings)
    {
        List<VisualizationLayer> layers = new();
        XElement? layersEl = vizEl.Element("layers");
        if (layersEl == null)
        {
            return [];
        }

        foreach (XElement layerEl in layersEl.Elements("layer"))
        {
            string? ink = (string?)layerEl.Attribute("ink");
            string? tag = (string?)layerEl.Attribute("tag");

            byte flags = 0;
            if (ParseBool(layerEl.Attribute("ignoreMouse")))
            {
                flags |= VisualizationLayer.FLAG_IGNORE_MOUSE;
            }

            layers.Add(new VisualizationLayer
            {
                Id = ParseUInt16(layerEl.Attribute("id")),
                X = ParseInt16(layerEl.Attribute("x")),
                Y = ParseInt16(layerEl.Attribute("y")),
                Z = ParseInt16(layerEl.Attribute("z")),
                Alpha = (ushort)((int?)layerEl.Attribute("alpha") ?? 255),
                InkIndex = ink != null ? strings.Add(ink) : VortexBundleFormat.NULL_STRING,
                TagIndex = tag != null ? strings.Add(tag) : VortexBundleFormat.NULL_STRING,
                Flags = flags,
            });
        }

        return layers.ToArray();
    }

    private static VisualizationColor[] ParseColors(XElement vizEl)
    {
        List<VisualizationColor> colors = new();
        XElement? colorsEl = vizEl.Element("colors");
        if (colorsEl == null)
        {
            return [];
        }

        foreach (XElement colorEl in colorsEl.Elements("color"))
        {
            VisualizationColor color = new()
            {
                ColorId = ParseUInt32(colorEl.Attribute("id")),
            };

            List<VisualizationColorLayer> layerColors = new();
            foreach (XElement clEl in colorEl.Elements("colorLayer"))
            {
                string? colorHex = (string?)clEl.Attribute("color");
                uint argb = 0xFF000000; // Default opaque black
                if (colorHex != null)
                {
                    if (uint.TryParse(colorHex.TrimStart('#'), NumberStyles.HexNumber, null, out uint parsed))
                    {
                        // If only RGB (6 hex digits), add full alpha
                        argb = colorHex.TrimStart('#').Length <= 6 ? (0xFF000000 | parsed) : parsed;
                    }
                }

                layerColors.Add(new VisualizationColorLayer
                {
                    LayerId = ParseUInt16(clEl.Attribute("id")), Color = argb,
                });
            }

            color.Layers = layerColors.ToArray();
            colors.Add(color);
        }

        return colors.ToArray();
    }

    private static VisualizationDirection[] ParseDirections(XElement vizEl)
    {
        List<VisualizationDirection> dirs = new();
        XElement? dirsEl = vizEl.Element("directions");
        if (dirsEl == null)
        {
            return [];
        }

        foreach (XElement dirEl in dirsEl.Elements("direction"))
        {
            VisualizationDirection dir = new()
            {
                DirectionId = ParseUInt16(dirEl.Attribute("id")),
            };

            List<VisualizationDirectionLayer> overrides = new();
            foreach (XElement layerEl in dirEl.Elements("layer"))
            {
                overrides.Add(new VisualizationDirectionLayer
                {
                    LayerId = ParseUInt16(layerEl.Attribute("id")), Z = ParseInt16(layerEl.Attribute("z")),
                });
            }

            dir.LayerOverrides = overrides.ToArray();
            dirs.Add(dir);
        }

        return dirs.ToArray();
    }

    private static VisualizationAnimation[] ParseAnimations(XElement vizEl)
    {
        List<VisualizationAnimation> anims = new();
        XElement? animsEl = vizEl.Element("animations");
        if (animsEl == null)
        {
            return [];
        }

        foreach (XElement animEl in animsEl.Elements("animation"))
        {
            VisualizationAnimation anim = new()
            {
                AnimationId = ParseUInt32(animEl.Attribute("id")),
            };

            List<VisualizationAnimSequence> sequences = new();
            foreach (XElement layerEl in animEl.Elements("animationLayer"))
            {
                VisualizationAnimSequence seq = new()
                {
                    LayerId = ParseUInt16(layerEl.Attribute("id")),
                    LoopCount = ParseUInt16(layerEl.Attribute("loopCount")),
                    Random = ParseBool(layerEl.Attribute("random")),
                };

                List<VisualizationAnimFrame> frames = new();
                foreach (XElement frameEl in layerEl.Elements("frameSequence").SelectMany(fs => fs.Elements("frame")))
                {
                    frames.Add(new VisualizationAnimFrame
                    {
                        Id = ParseUInt16(frameEl.Attribute("id")),
                        OffsetX = ParseInt16(frameEl.Attribute("x")),
                        OffsetY = ParseInt16(frameEl.Attribute("y")),
                    });
                }

                // Also check direct <frame> children
                foreach (XElement frameEl in layerEl.Elements("frame"))
                {
                    frames.Add(new VisualizationAnimFrame
                    {
                        Id = ParseUInt16(frameEl.Attribute("id")),
                        OffsetX = ParseInt16(frameEl.Attribute("x")),
                        OffsetY = ParseInt16(frameEl.Attribute("y")),
                    });
                }

                seq.Frames = frames.ToArray();
                sequences.Add(seq);
            }

            anim.Sequences = sequences.ToArray();
            anims.Add(anim);
        }

        return anims.ToArray();
    }

    private static VisualizationPosture[] ParsePostures(XElement vizEl, StringTableBuilder strings)
    {
        List<VisualizationPosture> postures = new();
        XElement? posturesEl = vizEl.Element("postures");
        if (posturesEl == null)
        {
            return [];
        }

        foreach (XElement pEl in posturesEl.Elements("posture"))
        {
            string? name = (string?)pEl.Attribute("id");
            if (name == null)
            {
                continue;
            }

            postures.Add(new VisualizationPosture
            {
                NameIndex = strings.Add(name), AnimationId = ParseUInt32(pEl.Attribute("animationId")),
            });
        }

        return postures.ToArray();
    }

    private static VisualizationGesture[] ParseGestures(XElement vizEl, StringTableBuilder strings)
    {
        List<VisualizationGesture> gestures = new();
        XElement? gesturesEl = vizEl.Element("gestures");
        if (gesturesEl == null)
        {
            return [];
        }

        foreach (XElement gEl in gesturesEl.Elements("gesture"))
        {
            string? name = (string?)gEl.Attribute("id");
            if (name == null)
            {
                continue;
            }

            gestures.Add(new VisualizationGesture
            {
                NameIndex = strings.Add(name), AnimationId = ParseUInt32(gEl.Attribute("animationId")),
            });
        }

        return gestures.ToArray();
    }

    private static ushort ParseUInt16(XAttribute? attr)
    {
        return attr != null && ushort.TryParse(attr.Value, out ushort v) ? v : (ushort)0;
    }

    private static short ParseInt16(XAttribute? attr)
    {
        return attr != null && short.TryParse(attr.Value, out short v) ? v : (short)0;
    }

    private static uint ParseUInt32(XAttribute? attr)
    {
        return attr != null && uint.TryParse(attr.Value, out uint v) ? v : 0u;
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
