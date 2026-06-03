using System;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Room.Utils;

namespace Vortex.Room.Object.Visualization.Utils;

/// @see com.sulake.room.object.visualization.utils.GraphicAssetCollection
public class GraphicAssetCollection : IGraphicAssetCollection
{
    private static readonly string[] PALETTE_ATTRIBUTES = ["id", "source"];

    private readonly Dictionary<string, GraphicAsset>? _assets = new();
    private IAssetLibrary? _assetLibrary;
    private readonly Dictionary<string, GraphicAssetPalette>? _palettes = new();
    private List<string>? _paletteAssetNames =
        [];
    private readonly Dictionary<string, XElement>? _paletteXmls = new();
    private readonly Dictionary<string, XElement>? _lazyAssets = new();

    public int ReferenceCount { get; private set; }

    public int LastReferenceTimeStamp { get; private set; }

    public IAssetLibrary? AssetLibrary
    {
        set => _assetLibrary = value;
    }

    public void Dispose()
    {
        if (_palettes != null)
        {
            foreach (GraphicAssetPalette palette in _palettes.Values)
            {
                palette.Dispose();
            }
            _palettes.Clear();
        }

        _paletteXmls?.Clear();

        if (_paletteAssetNames != null)
        {
            DisposePaletteAssets(true);
            _paletteAssetNames = null;
        }

        if (_assets != null)
        {
            foreach (GraphicAsset asset in _assets.Values)
            {
                asset.Recycle();
            }
            _assets.Clear();
        }

        _lazyAssets?.Clear();
        _assetLibrary = null;
    }

    public void AddReference()
    {
        ReferenceCount++;
        LastReferenceTimeStamp = System.Environment.TickCount;
    }

    public void RemoveReference()
    {
        ReferenceCount--;
        if (ReferenceCount <= 0)
        {
            ReferenceCount = 0;
            LastReferenceTimeStamp = System.Environment.TickCount;
            DisposePaletteAssets(false);
        }
    }

    public bool Define(XElement xml)
    {
        if (xml == null)
        {
            return false;
        }

        IEnumerable<XElement> assets = xml.Elements("asset");
        IEnumerable<XElement> palettes = xml.Elements("palette");
        DefinePalettes(palettes);
        DefineAssets(assets);
        return true;
    }

    private void DefineAssets(IEnumerable<XElement> assetElements)
    {
        foreach (XElement element in assetElements)
        {
            string name = (string?)element.Attribute("name") ?? "";
            if (name.Length == 0)
            {
                continue;
            }

            string source = (string?)element.Attribute("source") ?? "";
            bool flipH = false;
            bool flipV = false;
            int offsetX = -((int?)element.Attribute("x") ?? 0);
            int offsetY = -((int?)element.Attribute("y") ?? 0);

            if (((int?)element.Attribute("flipH") ?? 0) > 0 && source.Length > 0)
            {
                flipH = true;
            }
            if (((int?)element.Attribute("flipV") ?? 0) > 0 && source.Length > 0)
            {
                flipV = true;
            }

            bool usesPalette = ((int?)element.Attribute("usesPalette") ?? 0) != 0;

            if (source.Length == 0)
            {
                source = name;
            }

            IAsset? asset = _assetLibrary?.GetAssetByName(source);
            if (asset != null)
            {
                if (!CreateAsset(name, source, asset, flipH, flipV, offsetX, offsetY, usesPalette))
                {
                    IGraphicAsset? existing = GetAsset(name);
                    if (existing != null && existing.AssetName != existing.LibraryAssetName)
                    {
                        ReplaceAsset(name, source, asset, flipH, flipV, offsetX, offsetY, usesPalette);
                    }
                }
            }
        }
    }

    private void DefinePalettes(IEnumerable<XElement> paletteElements)
    {
        foreach (XElement element in paletteElements)
        {
            if (!XMLValidator.CheckRequiredAttributes(element, PALETTE_ATTRIBUTES))
            {
                continue;
            }

            string id = (string?)element.Attribute("id") ?? "";
            string source = (string?)element.Attribute("source") ?? "";

            if (_palettes!.ContainsKey(id))
            {
                continue;
            }

            IAsset? asset = _assetLibrary?.GetAssetByName(source);
            if (asset?.Content == null)
            {
                Logger.Warn($"Palette asset was null: {source}");
                continue;
            }

            byte[]? paletteData;
            if (asset.Content is byte[] bytes)
            {
                paletteData = bytes;
            }
            else
            {
                continue;
            }

            int primaryColor = 0xFFFFFF;
            int secondaryColor = 0xFFFFFF;

            string color1Str = (string?)element.Attribute("color1") ?? "";
            if (color1Str.Length > 0)
            {
                primaryColor = Convert.ToInt32(color1Str, 16);
                secondaryColor = primaryColor;
            }

            string color2Str = (string?)element.Attribute("color2") ?? "";
            if (color2Str.Length > 0)
            {
                secondaryColor = Convert.ToInt32(color2Str, 16);
            }

            GraphicAssetPalette palette = new(paletteData, primaryColor, secondaryColor);
            _palettes[id] = palette;
            _paletteXmls![id] = element;
        }
    }

    protected bool CreateAsset(string name, string libraryName, IAsset asset,
        bool flipH, bool flipV, int offsetX, int offsetY, bool usesPalette)
    {
        if (_assets!.ContainsKey(name))
        {
            return false;
        }
        if (_lazyAssets!.ContainsKey(name))
        {
            return false;
        }
        GraphicAsset graphicAsset = GraphicAsset.Allocate(name, libraryName, asset, flipH, flipV, offsetX, offsetY, usesPalette);
        _assets[name] = graphicAsset;
        return true;
    }

    protected bool ReplaceAsset(string name, string libraryName, IAsset asset,
        bool flipH, bool flipV, int offsetX, int offsetY, bool usesPalette)
    {
        if (_assets!.Remove(name, out GraphicAsset? existing))
        {
            existing.Recycle();
        }
        else
        {
            _lazyAssets!.Remove(name);
        }
        return CreateAsset(name, libraryName, asset, flipH, flipV, offsetX, offsetY, usesPalette);
    }

    public IGraphicAsset? GetAsset(string name)
    {
        if (_assets!.TryGetValue(name, out GraphicAsset? asset))
        {
            return asset;
        }

        if (_lazyAssets!.Remove(name, out XElement? lazyXml))
        {
            string source = (string?)lazyXml.Attribute("source") ?? "";
            bool flipH = false;
            bool flipV = false;
            int offsetX = -((int?)lazyXml.Attribute("x") ?? 0);
            int offsetY = -((int?)lazyXml.Attribute("y") ?? 0);

            if (((int?)lazyXml.Attribute("flipH") ?? 0) > 0 && source.Length > 0)
            {
                flipH = true;
            }
            if (((int?)lazyXml.Attribute("flipV") ?? 0) > 0 && source.Length > 0)
            {
                flipV = true;
            }

            bool usesPalette = ((int?)lazyXml.Attribute("usesPalette") ?? 0) != 0;

            if (source.Length == 0)
            {
                source = name;
            }

            IAsset? libAsset = _assetLibrary?.GetAssetByName(source);
            if (libAsset != null)
            {
                if (CreateAsset(name, source, libAsset, flipH, flipV, offsetX, offsetY, usesPalette))
                {
                    return _assets[name];
                }
                IGraphicAsset? existing = GetAsset(name);
                if (existing != null && existing.AssetName != existing.LibraryAssetName)
                {
                    ReplaceAsset(name, source, libAsset, flipH, flipV, offsetX, offsetY, usesPalette);
                    if (_assets.TryGetValue(name, out GraphicAsset? replaced))
                    {
                        return replaced;
                    }
                }
            }
        }

        return null;
    }

    public IGraphicAsset? GetAssetWithPalette(string name, string palette)
    {
        string paletteKey = name + "@" + palette;
        IGraphicAsset? result = GetAsset(paletteKey);
        if (result != null)
        {
            return result;
        }

        IGraphicAsset? baseAsset = GetAsset(name);
        if (baseAsset == null || !baseAsset.UsesPalette)
        {
            return baseAsset;
        }

        string libraryKey = baseAsset.LibraryAssetName + "@" + palette;
        BitmapDataAsset? libraryAsset = GetLibraryAsset(libraryKey);

        if (libraryAsset == null)
        {
            if (baseAsset.Asset?.Content is Image srcImage)
            {
                GraphicAssetPalette? paletteObj = GetPalette(palette);
                if (paletteObj == null)
                {
                    return baseAsset;
                }

                Image? colorized = (Image)srcImage.Duplicate();
                paletteObj.ColorizeBitmap(colorized);

                libraryAsset = AddLibraryAsset(libraryKey, colorized);
                if (libraryAsset == null)
                {
                    return null;
                }
            }
        }

        _paletteAssetNames!.Add(paletteKey);
        CreateAsset(paletteKey, libraryKey, libraryAsset!,
            baseAsset.FlipH, baseAsset.FlipV,
            baseAsset.OriginalOffsetX, baseAsset.OriginalOffsetY, false);
        return GetAsset(paletteKey);
    }

    public string[]? GetPaletteNames()
    {
        if (_palettes == null)
        {
            return null;
        }
        string[] keys = new string[_palettes.Count];
        _palettes.Keys.CopyTo(keys, 0);
        return keys;
    }

    public int[]? GetPaletteColors(string name)
    {
        GraphicAssetPalette? palette = GetPalette(name);
        if (palette != null)
        {
            return [palette.PrimaryColor, palette.SecondaryColor];
        }
        return null;
    }

    public XElement? GetPaletteXml(string name)
    {
        if (_paletteXmls != null && _paletteXmls.TryGetValue(name, out XElement? xml))
        {
            return xml;
        }
        return null;
    }

    private GraphicAssetPalette? GetPalette(string name)
    {
        if (_palettes != null && _palettes.TryGetValue(name, out GraphicAssetPalette? palette))
        {
            return palette;
        }
        return null;
    }

    public bool AddAsset(string name, Image data, bool overwrite,
        int offsetX = 0, int offsetY = 0, bool flipH = false, bool flipV = false)
    {
        if (name == null || data == null || _assetLibrary == null)
        {
            return false;
        }

        BitmapDataAsset? libraryAsset = GetLibraryAsset(name);
        if (libraryAsset == null)
        {
            AssetTypeDeclaration? declaration = _assetLibrary.GetAssetTypeDeclarationByClass(typeof(BitmapDataAsset));
            BitmapDataAsset newAsset = new(declaration);
            _assetLibrary.SetAsset(name, newAsset);
            newAsset.SetUnknownContent(data);
            return CreateAsset(name, name, newAsset, flipH, flipV, offsetX, offsetY, false);
        }

        if (overwrite)
        {
            if (libraryAsset.Content is Image existingImage && existingImage != data)
            {
                // AS3 disposes old BitmapData; Godot Image is GC'd
            }
            libraryAsset.SetUnknownContent(data);
            return true;
        }

        return false;
    }

    public void DisposeAsset(string name)
    {
        if (_assets!.Remove(name, out GraphicAsset? asset))
        {
            BitmapDataAsset? libraryAsset = GetLibraryAsset(asset.LibraryAssetName!);
            if (libraryAsset != null)
            {
                _assetLibrary?.RemoveAsset(libraryAsset);
                libraryAsset.Dispose();
            }
            asset.Recycle();
        }
        else
        {
            _lazyAssets!.Remove(name);
        }
    }

    private BitmapDataAsset? GetLibraryAsset(string name)
    {
        return _assetLibrary?.GetAssetByName(name) as BitmapDataAsset;
    }

    private BitmapDataAsset? AddLibraryAsset(string name, Image data)
    {
        BitmapDataAsset? existing = GetLibraryAsset(name);
        if (existing != null)
        {
            return null;
        }
        AssetTypeDeclaration? declaration = _assetLibrary!.GetAssetTypeDeclarationByClass(typeof(BitmapDataAsset));
        BitmapDataAsset asset = new(declaration);
        _assetLibrary.SetAsset(name, asset);
        asset.SetUnknownContent(data);
        return asset;
    }

    private void DisposePaletteAssets(bool force)
    {
        if (_paletteAssetNames == null)
        {
            return;
        }
        if (force || _paletteAssetNames.Count > 10)
        {
            foreach (string name in _paletteAssetNames)
            {
                DisposeAsset(name);
            }
            _paletteAssetNames.Clear();
        }
    }
}
