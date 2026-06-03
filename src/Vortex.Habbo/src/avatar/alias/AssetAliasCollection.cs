// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/alias/AssetAliasCollection.as

using System.Xml.Linq;

using Vortex.Core.Assets;

namespace Vortex.Habbo.Avatar.Alias;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/alias/AssetAliasCollection.as
public class AssetAliasCollection
{
    private AssetLibraryCollection? _assets;
    private Dictionary<string, AssetAlias>? _aliases;
    private readonly IAvatarRenderManager? _avatarRenderManager;

    /// @see AssetAliasCollection.as::AssetAliasCollection
    public AssetAliasCollection(IAvatarRenderManager renderManager, AssetLibraryCollection assets)
    {
        _avatarRenderManager = renderManager;
        _aliases = new Dictionary<string, AssetAlias>();
        _assets = assets;
    }

    /// @see AssetAliasCollection.as::dispose
    public void Dispose()
    {
        _assets = null;
        _aliases = null;
    }

    /// @see AssetAliasCollection.as::reset
    public void Reset()
    {
        Init();
    }

    /// @see AssetAliasCollection.as::onAvatarAssetsLibraryReady
    public void OnAvatarAssetsLibraryReady(string libraryName)
    {
        if (_assets == null || _aliases == null)
        {
            return;
        }

        IAssetLibrary? library = _assets.GetAssetLibraryByPartialUrl("/" + libraryName + ".swf");

        if (library?.Manifest == null)
        {
            return;
        }

        foreach (XElement aliasXml in library.Manifest.Descendants("alias"))
        {
            string name = (string?)aliasXml.Attribute("name") ?? "";
            _aliases[name] = new AssetAlias(aliasXml);
        }
    }

    /// @see AssetAliasCollection.as::init
    public void Init()
    {
        if (_assets == null || _aliases == null)
        {
            return;
        }

        List<XElement> manifests = _assets.GetManifests();

        foreach (XElement manifest in manifests)
        {
            foreach (XElement aliasXml in manifest.Descendants("alias"))
            {
                string name = (string?)aliasXml.Attribute("name") ?? "";
                _aliases[name] = new AssetAlias(aliasXml);
            }
        }
    }

    /// @see AssetAliasCollection.as::addAlias
    public void AddAlias(string name, string link, bool flipH = false, bool flipV = false)
    {
        if (_aliases == null)
        {
            return;
        }

        XElement xml = new("alias",
            new XAttribute("name", name),
            new XAttribute("link", link),
            new XAttribute("fliph", flipH ? "1" : "0"),
            new XAttribute("flipv", flipV ? "1" : "0"));

        _aliases[name] = new AssetAlias(xml);
    }

    /// @see AssetAliasCollection.as::hasAlias
    public bool HasAlias(string name)
    {
        return _aliases != null && _aliases.ContainsKey(name);
    }

    /// @see AssetAliasCollection.as::getAssetName
    /// Recursive resolution with max 5 hops to prevent infinite loops.
    public string GetAssetName(string name)
    {
        string resolved = name;
        int maxHops = 5;

        while (HasAlias(resolved) && maxHops >= 0)
        {
            resolved = _aliases![resolved].Link;
            maxHops--;
        }

        return resolved;
    }

    /// @see AssetAliasCollection.as::getAssetByName
    public IAsset? GetAssetByName(string name)
    {
        name = GetAssetName(name);
        return _assets?.GetAssetByName(name);
    }
}
