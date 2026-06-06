// Godot adaptation: IAssetLibrary backed by HabboAssetResolver filesystem lookups.
// Used by the WindowSystemCreation test harness, which has no component asset library.

using System;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;

namespace Vortex.Habbo.Window.Utils;

/// <summary>
/// Godot adaptation: wraps HabboAssetResolver as an IAssetLibrary.
/// Asset names ending in "_xml" resolve to XElement; all others resolve to Image.
/// Only GetAssetByName is implemented; all mutating methods are no-ops.
/// </summary>
public sealed class HabboFileSystemAssetLibrary : IAssetLibrary
{
    public string? Url => null;
    public string Name => "HabboFileSystem";
    public bool IsReady => true;
    public int NumAssets => 0;
    public XElement? Manifest => null;
    public IList<string> NameArray => [];
    public bool disposed => false;

    /// @see class_3503.as::parse — param2.getAssetByName(name)
    public IAsset? GetAssetByName(string name)
    {
        object? content = name.EndsWith("_xml", StringComparison.Ordinal)
            ? (object?)HabboAssetResolver.LoadXmlAsset(name)
            : HabboAssetResolver.LoadImageAsset(name);

        return content is not null ? new ContentAsset(content) : null;
    }

    public void Dispose() { }
    public IAsset? GetAssetByIndex(int p)
    {
        return null;
    }

    public IAsset? GetAssetByContent(object p)
    {
        return null;
    }

    public int GetAssetIndex(IAsset p)
    {
        return -1;
    }

    public bool HasAsset(string name)
    {
        return GetAssetByName(name) is not null;
    }

    public bool SetAsset(string p1, IAsset p2, bool p3 = true)
    {
        return false;
    }

    public IAsset? CreateAsset(string p1, AssetTypeDeclaration p2)
    {
        return null;
    }

    public IAsset? RemoveAsset(IAsset p1)
    {
        return null;
    }

    public bool RegisterAssetTypeDeclaration(AssetTypeDeclaration p1, bool p2 = true)
    {
        return false;
    }

    public AssetTypeDeclaration? GetAssetTypeDeclarationByMimeType(string p1, bool p2 = true)
    {
        return null;
    }

    public AssetTypeDeclaration? GetAssetTypeDeclarationByClass(Type p1, bool p2 = true)
    {
        return null;
    }

    public AssetTypeDeclaration? GetAssetTypeDeclarationByFileName(string p1, bool p2 = true)
    {
        return null;
    }

    public void LoadFromFile(object p1, bool p2 = true) { }
    public AssetLoaderStruct? LoadAssetFromFile(string p1, string p2, string? p3 = null, int p4 = -1)
    {
        return null;
    }

    public bool LoadFromResource(XElement p1, Type p2)
    {
        return false;
    }

    public void Unload() { }

    private sealed class ContentAsset(object content) : IAsset
    {
        public string? Url => null;
        public object? Content => content;
        public Rect2? Rectangle => null;
        public AssetTypeDeclaration? Declaration => null;
        public bool disposed => false;
        public void Dispose() { }
        public void SetUnknownContent(object? p) { }
        public void SetFromOtherAsset(IAsset p) { }
        public void SetParamsDesc(IEnumerable<XElement>? p) { }
    }
}
