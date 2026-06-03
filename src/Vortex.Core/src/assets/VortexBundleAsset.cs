// Godot adaptation: asset wrapper for .vortex bundle data.
// Follows the same IAsset pattern as BitmapDataAsset, TextAsset, etc.

using System.Text;
using System.Xml.Linq;

using Godot;

using Vortex.Bundle.Data;
using Vortex.Core.Assets.Bundle;

namespace Vortex.Core.Assets;

/// <summary>
/// IAsset implementation for .vortex bundles.
/// Wraps VortexBundleData and provides a method to populate a standard
/// AssetLibrary with individual BitmapDataAsset entries.
/// </summary>
public class VortexBundleAsset(AssetTypeDeclaration? declaration = null, string? url = null) : IAsset
{
    public const string MIME_TYPE = "application/x-vortex-bundle";

    public string? Name { get; set; }

    public string? Url => url;

    public AssetTypeDeclaration? Declaration => declaration;

    public object? Content => BundleData;

    public Rect2? Rectangle => null;

    public bool disposed { get; private set; }

    public VortexBundleData? BundleData { get; private set; }

    public VortexSpritesheet? Spritesheet { get; private set; }

    public void SetUnknownContent(object? content)
    {
        if (content is not VortexBundleData data)
        {
            Logging.Logger.Warn(
                $"[VortexBundleAsset] SetUnknownContent: content is {content?.GetType().Name ?? "null"}, not VortexBundleData");
            return;
        }

        BundleData = data;
        Spritesheet = VortexBundleLoader.CreateSpritesheet(data);

        Logging.Logger.Info(
            $"[VortexBundleAsset] SetUnknownContent: v{data.Version} flags=0x{data.Flags:X}, assets={data.Assets?.Length ?? 0}, aliases={data.Aliases?.Length ?? 0}, strings={data.StringTable?.Count ?? 0}, sheetMeta={data.SpritesheetMeta != null}, sheetImg={data.SpritesheetImage?.Length ?? 0}b, spritesheet={Spritesheet != null}");
    }

    public void SetFromOtherAsset(IAsset other)
    {
        if (other is not VortexBundleAsset vba)
        {
            return;
        }

        BundleData = vba.BundleData;
        Spritesheet = vba.Spritesheet;
    }

    public void SetParamsDesc(IEnumerable<XElement>? param1) { }

    /// <summary>
    /// Populates the given AssetLibrary with sprites/aliases and reconstructs
    /// XML metadata assets (index, visualization, logic, binaryData) from
    /// the binary bundle data.
    /// </summary>
    public void PopulateLibrary(AssetLibrary library, string typeName)
    {
        PopulateLibrary(library);

        if (BundleData == null || BundleData.StringTable == null)
        {
            return;
        }

        // Build and store {type}binaryData XML (asset definitions for GraphicAssetCollection.Define)
        XElement assetsXml = BuildAssetsXml(BundleData, BundleData.StringTable);
        XmlAsset assetsAsset = new();
        assetsAsset.SetUnknownContent(assetsXml);
        library.SetAsset(typeName + "binaryData", assetsAsset);

        // Build and store {type}_visualization XML
        XElement visualizationXml = BuildVisualizationXml(typeName, BundleData, BundleData.StringTable);
        XmlAsset visAsset = new();
        visAsset.SetUnknownContent(visualizationXml);
        library.SetAsset(typeName + "_visualization", visAsset);

        // Build and store {type}_logic XML
        XElement logicXml = BuildLogicXml(typeName, BundleData, BundleData.StringTable);
        XmlAsset logicAsset = new();
        logicAsset.SetUnknownContent(logicXml);
        library.SetAsset(typeName + "_logic", logicAsset);

        // Build and store {type}_index and index XML
        XElement indexXml = BuildIndexXml(typeName, BundleData);
        XmlAsset indexAsset = new();
        indexAsset.SetUnknownContent(indexXml);
        library.SetAsset(typeName + "_index", indexAsset);

        XmlAsset indexAsset2 = new();
        indexAsset2.SetUnknownContent(indexXml);
        library.SetAsset("index", indexAsset2);

        // Overwrite with raw XML blobs from the bundle (preserves original XML
        // like room-specific <wallData>/<floorData> that BuildVisualizationXml can't reconstruct)
        if (BundleData.RawData != null)
        {
            foreach ((string blobName, byte[] bytes) in BundleData.RawData)
            {
                string xmlStr = Encoding.UTF8.GetString(bytes);
                XElement element = XElement.Parse(xmlStr);
                XmlAsset xmlAsset = new();
                xmlAsset.SetUnknownContent(element);

                // Normalize blob name: strip typeName prefix if present
                // e.g. "room_visualization" with typeName "room" → "visualization"
                string suffix = blobName;
                string typePrefix = typeName + "_";

                if (suffix.StartsWith(typePrefix, System.StringComparison.Ordinal))
                {
                    suffix = suffix[typePrefix.Length..];
                }

                string libraryKey = suffix switch
                {
                    "visualization" => typeName + "_visualization",
                    "assets" => typeName + "binaryData",
                    "logic" => typeName + "_logic",
                    "index" => typeName + "_index",
                    _ => typeName + "_" + blobName,
                };
                library.SetAsset(libraryKey, xmlAsset);
            }
        }
    }

    /// <summary>
    /// Populates the given AssetLibrary with individual BitmapDataAsset entries
    /// extracted from the .vortex bundle spritesheet.
    /// Follows the same pattern as AssetLibrary.FetchLibraryContents.
    /// </summary>
    public void PopulateLibrary(AssetLibrary library)
    {
        if (BundleData == null || Spritesheet == null || BundleData.StringTable == null)
        {
            Logging.Logger.Warn(
                $"[VortexBundleAsset] PopulateLibrary bail: bundleData={BundleData != null}, spritesheet={Spritesheet != null}, stringTable={BundleData?.StringTable != null}");
            return;
        }

        StringTable? strings = BundleData.StringTable;

        // Register direct assets
        if (BundleData.Assets is { Length: > 0 })
        {
            foreach (AssetEntry entry in BundleData.Assets)
            {
                string? name = strings.Resolve(entry.NameIndex);

                if (name == null)
                {
                    continue;
                }

                BitmapDataAsset? bitmap = ExtractBitmapAsset(name, entry);

                if (bitmap != null)
                {
                    library.SetAsset(name, bitmap);
                }
            }
        }
        else if (BundleData.SpritesheetMeta is { Frames.Length: > 0 })
        {
            // Fallback: no explicit ASSETS section but spritesheet has named frames.
            // Create BitmapDataAsset entries directly from frame metadata.
            // This handles bundles where the converter didn't produce an assets section
            // (e.g. avatar SWFs without embedded assets XML).
            foreach (FrameData frame in BundleData.SpritesheetMeta.Frames)
            {
                string? name = strings.Resolve(frame.NameIndex);

                if (name == null)
                {
                    continue;
                }

                Image? image = Spritesheet.ExtractFrame(name);

                if (image == null)
                {
                    continue;
                }

                BitmapDataAsset asset = new(image)
                {
                    Name = name,
                };

                library.SetAsset(name, asset);
            }
        }

        // Register aliases
        if (BundleData.Aliases != null)
        {
            foreach (AliasEntry alias in BundleData.Aliases)
            {
                string? aliasName = strings.Resolve(alias.NameIndex);
                string? targetName = strings.Resolve(alias.LinkIndex);

                if (aliasName == null || targetName == null)
                {
                    continue;
                }

                IAsset? refAsset = library.GetAssetByName(targetName);

                if (refAsset == null)
                {
                    continue;
                }

                BitmapDataAsset bitmap = new()
                {
                    Name = aliasName,
                };
                bitmap.SetFromOtherAsset(refAsset);

                if (alias.FlipH || alias.FlipV)
                {
                    List<XElement> flipParams = new();

                    if (alias.FlipH)
                    {
                        flipParams.Add(
                            new XElement(
                                "param",
                                new XAttribute("key", "flipH"),
                                new XAttribute("value", "true")
                            )
                        );
                    }

                    if (alias.FlipV)
                    {
                        flipParams.Add(
                            new XElement(
                                "param",
                                new XAttribute("key", "flipV"),
                                new XAttribute("value", "true")
                            )
                        );
                    }

                    bitmap.SetParamsDesc(flipParams);
                }

                library.SetAsset(aliasName, bitmap);
            }
        }
    }

    private BitmapDataAsset? ExtractBitmapAsset(string name, AssetEntry entry)
    {
        if (Spritesheet == null)
        {
            return null;
        }

        Image? image = Spritesheet.ExtractFrame(name);

        if (image == null)
        {
            return null;
        }

        if (entry.FlipH)
        {
            image.FlipX();
        }

        if (entry.FlipV)
        {
            image.FlipY();
        }

        BitmapDataAsset asset = new(image)
        {
            Name = name,
        };

        if (entry.OffsetX != 0 || entry.OffsetY != 0)
        {
            asset.SetParamsDesc(
                [
                    new XElement(
                        "param",
                        new XAttribute("key", "offset"),
                        new XAttribute("value", $"{entry.OffsetX},{entry.OffsetY}")
                    ),
                ]
            );
        }

        return asset;
    }

    /// <summary>
    /// Builds the assets XML consumed by <c>GraphicAssetCollection.Define()</c>.
    /// Format: <c>&lt;assets&gt;&lt;asset name="..." source="..." x="..." y="..." flipH="..." flipV="..." usesPalette="..."/&gt;&lt;/assets&gt;</c>
    /// </summary>
    private static XElement BuildAssetsXml(VortexBundleData data, StringTable strings)
    {
        XElement root = new("assets");

        if (data.Assets != null)
        {
            foreach (AssetEntry entry in data.Assets)
            {
                string? name = strings.Resolve(entry.NameIndex);

                if (name == null)
                {
                    continue;
                }

                XElement el = new("asset", new XAttribute("name", name));

                string? source = strings.Resolve(entry.SourceIndex);

                if (source != null && source != name)
                {
                    el.Add(new XAttribute("source", source));
                }

                if (entry.OffsetX != 0)
                {
                    el.Add(new XAttribute("x", entry.OffsetX));
                }

                if (entry.OffsetY != 0)
                {
                    el.Add(new XAttribute("y", entry.OffsetY));
                }

                if (entry.FlipH)
                {
                    el.Add(new XAttribute("flipH", 1));
                }

                if (entry.FlipV)
                {
                    el.Add(new XAttribute("flipV", 1));
                }

                if (entry.UsesPalette)
                {
                    el.Add(new XAttribute("usesPalette", 1));
                }

                root.Add(el);
            }
        }

        if (data.Palettes != null)
        {
            foreach (PaletteData palette in data.Palettes)
            {
                string? source = strings.Resolve(palette.SourceIndex);

                if (source == null)
                {
                    continue;
                }

                XElement el = new("palette",
                    new XAttribute("id", palette.Id),
                    new XAttribute("source", source));

                string? color1 = strings.Resolve(palette.Color1Index);

                if (color1 != null)
                {
                    el.Add(new XAttribute("color1", color1));
                }

                string? color2 = strings.Resolve(palette.Color2Index);

                if (color2 != null)
                {
                    el.Add(new XAttribute("color2", color2));
                }

                root.Add(el);
            }
        }

        return root;
    }

    /// <summary>
    /// Builds the visualization XML consumed by <c>FurnitureVisualizationData.DefineVisualizations()</c>.
    /// Format: <c>&lt;visualizationData type="..."&gt;&lt;graphics&gt;&lt;visualization size="..." layerCount="..." angle="..."&gt;...&lt;/visualization&gt;&lt;/graphics&gt;&lt;/visualizationData&gt;</c>
    /// </summary>
    private static XElement BuildVisualizationXml(string typeName, VortexBundleData data, StringTable strings)
    {
        XElement root = new("visualizationData", new XAttribute("type", typeName));
        XElement graphics = new("graphics");
        root.Add(graphics);

        if (data.Visualizations == null)
        {
            return root;
        }

        foreach (VisualizationData vis in data.Visualizations)
        {
            XElement vizEl = new("visualization",
                new XAttribute("size", vis.Size),
                new XAttribute("layerCount", vis.LayerCount),
                new XAttribute("angle", vis.Angle));

            // Layers (default direction)
            if (vis.Layers.Length > 0)
            {
                XElement layersEl = new("layers");

                foreach (VisualizationLayer layer in vis.Layers)
                {
                    layersEl.Add(BuildLayerElement(layer, strings));
                }

                vizEl.Add(layersEl);
            }

            // Directions (per-direction layer overrides)
            if (vis.Directions.Length > 0)
            {
                XElement directionsEl = new("directions");

                foreach (VisualizationDirection dir in vis.Directions)
                {
                    XElement dirEl = new("direction", new XAttribute("id", dir.DirectionId));

                    foreach (VisualizationDirectionLayer layerOverride in dir.LayerOverrides)
                    {
                        XElement layerEl = new("layer", new XAttribute("id", layerOverride.LayerId));

                        if (layerOverride.Z != 0)
                        {
                            layerEl.Add(new XAttribute("z", layerOverride.Z));
                        }

                        dirEl.Add(layerEl);
                    }

                    directionsEl.Add(dirEl);
                }

                vizEl.Add(directionsEl);
            }

            // Colors
            if (vis.Colors.Length > 0)
            {
                XElement colorsEl = new("colors");

                foreach (VisualizationColor color in vis.Colors)
                {
                    XElement colorEl = new("color", new XAttribute("id", color.ColorId));

                    foreach (VisualizationColorLayer cl in color.Layers)
                    {
                        colorEl.Add(new XElement("colorLayer",
                            new XAttribute("id", cl.LayerId),
                            new XAttribute("color", (cl.Color & 0xFFFFFF).ToString("X6"))));
                    }

                    colorsEl.Add(colorEl);
                }

                vizEl.Add(colorsEl);
            }

            // Animations
            if (vis.Animations.Length > 0)
            {
                XElement animationsEl = new("animations");

                foreach (VisualizationAnimation anim in vis.Animations)
                {
                    XElement animEl = new("animation", new XAttribute("id", anim.AnimationId));

                    foreach (VisualizationAnimSequence seq in anim.Sequences)
                    {
                        XElement layerEl = new("animationLayer",
                            new XAttribute("id", seq.LayerId));

                        if (seq.LoopCount != 1)
                        {
                            layerEl.Add(new XAttribute("loopCount", seq.LoopCount));
                        }

                        if (seq.Random)
                        {
                            layerEl.Add(new XAttribute("random", 1));
                        }

                        if (seq.Frames.Length > 0)
                        {
                            XElement frameSeqEl = new("frameSequence");

                            foreach (VisualizationAnimFrame frame in seq.Frames)
                            {
                                XElement frameEl = new("frame", new XAttribute("id", frame.Id));

                                if (frame.OffsetX != 0)
                                {
                                    frameEl.Add(new XAttribute("x", frame.OffsetX));
                                }

                                if (frame.OffsetY != 0)
                                {
                                    frameEl.Add(new XAttribute("y", frame.OffsetY));
                                }

                                frameSeqEl.Add(frameEl);
                            }

                            layerEl.Add(frameSeqEl);
                        }

                        animEl.Add(layerEl);
                    }

                    animationsEl.Add(animEl);
                }

                vizEl.Add(animationsEl);
            }

            graphics.Add(vizEl);
        }

        return root;
    }

    private static XElement BuildLayerElement(VisualizationLayer layer, StringTable strings)
    {
        XElement el = new("layer", new XAttribute("id", layer.Id));

        string? tag = strings.Resolve(layer.TagIndex);

        if (tag != null)
        {
            el.Add(new XAttribute("tag", tag));
        }

        string? ink = strings.Resolve(layer.InkIndex);

        if (ink != null)
        {
            el.Add(new XAttribute("ink", ink));
        }

        if (layer.Alpha != 255)
        {
            el.Add(new XAttribute("alpha", layer.Alpha));
        }

        if (layer.IgnoreMouse)
        {
            el.Add(new XAttribute("ignoreMouse", 1));
        }

        if (layer.X != 0)
        {
            el.Add(new XAttribute("x", layer.X));
        }

        if (layer.Y != 0)
        {
            el.Add(new XAttribute("y", layer.Y));
        }

        if (layer.Z != 0)
        {
            el.Add(new XAttribute("z", layer.Z));
        }

        return el;
    }

    /// <summary>
    /// Builds the logic XML consumed by <c>FurnitureLogic.Initialize()</c>.
    /// Format: <c>&lt;objectData type="..."&gt;&lt;model&gt;&lt;dimensions .../&gt;&lt;directions&gt;...&lt;/directions&gt;&lt;/model&gt;&lt;action .../&gt;&lt;/objectData&gt;</c>
    /// </summary>
    private static XElement BuildLogicXml(string typeName, VortexBundleData data, StringTable strings)
    {
        XElement root = new("objectData", new XAttribute("type", typeName));

        LogicData? logic = data.Logic;

        if (logic == null || !logic.HasModel)
        {
            return root;
        }

        XElement model = new("model");
        root.Add(model);

        XElement dimensions = new("dimensions",
            new XAttribute("x", logic.DimensionX),
            new XAttribute("y", logic.DimensionY),
            new XAttribute("z", logic.DimensionZ));

        if (logic.CenterZ != 0)
        {
            dimensions.Add(new XAttribute("centerZ", logic.CenterZ));
        }

        model.Add(dimensions);

        if (logic.Directions.Length > 0)
        {
            XElement directionsEl = new("directions");

            foreach (ushort dir in logic.Directions)
            {
                directionsEl.Add(new XElement("direction", new XAttribute("id", dir)));
            }

            model.Add(directionsEl);
        }

        if (logic.HasAction)
        {
            XElement action = new("action");

            string? link = strings.Resolve(logic.ActionLinkIndex);

            if (link != null)
            {
                action.Add(new XAttribute("link", link));
            }

            if (logic.ActionStartState != 0)
            {
                action.Add(new XAttribute("startState", logic.ActionStartState));
            }

            root.Add(action);
        }

        return root;
    }

    /// <summary>
    /// Builds the index XML used by <c>GetVisualizationType()</c> / <c>GetLogicType()</c>.
    /// Format: <c>&lt;object type="..." visualization="..." logic="..."/&gt;</c>
    /// </summary>
    private static XElement BuildIndexXml(string typeName, VortexBundleData data)
    {
        // Determine visualization and logic type from bundle metadata.
        // For most furniture, the type name is used directly.
        string visType = typeName;
        string logicType = typeName;

        // If the bundle has visualization data, check for animations to pick the right vis type.
        if (data.Visualizations != null)
        {
            bool hasAnimations = false;

            foreach (VisualizationData vis in data.Visualizations)
            {
                if (vis.Animations.Length > 0)
                {
                    hasAnimations = true;
                    break;
                }
            }

            // For generic furniture, map to the standard visualization types
            if (visType != "room" && visType != "tile_cursor" && visType != "selection_arrow")
            {
                visType = hasAnimations ? "furniture_animated" : "furniture";
            }
        }

        // Map logic types for standard furniture
        if (logicType != "room" && logicType != "tile_cursor" && logicType != "selection_arrow")
        {
            logicType = "furniture_multistate";
        }

        return new XElement("object",
            new XAttribute("type", typeName),
            new XAttribute("visualization", visType),
            new XAttribute("logic", logicType));
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        Spritesheet?.Dispose();
        BundleData = null;
    }
}
