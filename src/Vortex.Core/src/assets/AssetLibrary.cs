// @see core/assets/AssetLibrary.as

using System;
using System.Linq;
using System.Xml.Linq;

using Vortex.Core.Assets.Loaders;

namespace Vortex.Core.Assets;

/// @see core/assets/AssetLibrary.as
/// Core asset library: manages assets by name, supports manifest XML loading and type registration.
public class AssetLibrary : IAssetLibrary
{
    /// @see AssetLibrary.as::ASSET_LIBRARY_READY
    public const string ASSET_LIBRARY_READY = "AssetLibraryReady";

    /// @see AssetLibrary.as::ASSET_LIBRARY_LOADED
    public const string ASSET_LIBRARY_LOADED = "AssetLibraryLoaded";

    /// @see AssetLibrary.as::ASSET_LIBRARY_UNLOADED
    public const string ASSET_LIBRARY_UNLOADED = "AssetLibraryUnloaded";

    /// @see AssetLibrary.as::ASSET_LIBRARY_LOAD_ERROR
    public const string ASSET_LIBRARY_LOAD_ERROR = "AssetLibraryLoadError";

    /// @see AssetLibrary.as::_sharedListOfTypesByMime
    private static Dictionary<string, AssetTypeDeclaration>? _sharedTypesByMime;
    private string? _name;
    private XElement? _manifest;

    /// @see AssetLibrary.as::var_159 — whether contents were fully extracted on load
    private bool _contentsExtracted = true;

    /// @see AssetLibrary.as::var_38 — asset storage by name
    private readonly Dictionary<string, IAsset> _assets = new(StringComparer.Ordinal);

    /// @see AssetLibrary.as::_assetNameArray
    private readonly List<string> _assetNameArray = [];

    /// @see AssetLibrary.as::_localListOfTypesByMime
    private readonly Dictionary<string, AssetTypeDeclaration> _localTypesByMime = new(StringComparer.OrdinalIgnoreCase);

    /// @see AssetLibrary.as::AssetLibrary
    public AssetLibrary(string name, XElement? manifest = null)
    {
        _name = name;
        _manifest = manifest;

        if (_sharedTypesByMime == null)
        {
            _sharedTypesByMime = new Dictionary<string, AssetTypeDeclaration>(StringComparer.OrdinalIgnoreCase);

            // @see AssetLibrary.as — 12 default MIME type registrations
            RegisterAssetTypeDeclaration(new AssetTypeDeclaration("application/octet-stream", typeof(UnknownAsset),
                typeof(BinaryFileLoader)));
            RegisterAssetTypeDeclaration(new AssetTypeDeclaration("application/x-shockwave-flash", typeof(DisplayAsset),
                typeof(BitmapFileLoader), "swf"));
            RegisterAssetTypeDeclaration(new AssetTypeDeclaration("application/x-font-truetype", typeof(TypeFaceAsset),
                typeof(BinaryFileLoader), "ttf", "otf"));
            RegisterAssetTypeDeclaration(new AssetTypeDeclaration("text/xml", typeof(XmlAsset), typeof(TextFileLoader), "xml"));
            RegisterAssetTypeDeclaration(new AssetTypeDeclaration("text/html", typeof(XmlAsset), typeof(TextFileLoader), "htm", "html"));
            RegisterAssetTypeDeclaration(new AssetTypeDeclaration("text/plain", typeof(TextAsset), typeof(TextFileLoader), "txt"));
            RegisterAssetTypeDeclaration(new AssetTypeDeclaration("image/jpeg", typeof(BitmapDataAsset), typeof(BitmapFileLoader), "jpg",
                "jpeg"));
            RegisterAssetTypeDeclaration(new AssetTypeDeclaration("image/gif", typeof(BitmapDataAsset), typeof(BitmapFileLoader), "gif"));
            RegisterAssetTypeDeclaration(new AssetTypeDeclaration("image/png", typeof(BitmapDataAsset), typeof(BitmapFileLoader), "png"));
            RegisterAssetTypeDeclaration(new AssetTypeDeclaration("image/tiff", typeof(BitmapDataAsset), typeof(BitmapFileLoader), "tif",
                "tiff"));
            RegisterAssetTypeDeclaration(new AssetTypeDeclaration("sound/mp3", typeof(SoundAsset), typeof(BinaryFileLoader), "mp3"));

            // Godot adaptation: .vortex bundle format for converted Habbo SWF assets
            RegisterAssetTypeDeclaration(new AssetTypeDeclaration(VortexBundleAsset.MIME_TYPE, typeof(VortexBundleAsset),
                typeof(VortexBundleFileLoader), "vortex"));
        }

        NumAssetLibraryInstances++;
    }

    /// @see AssetLibrary.as::get numAssetLibraryInstances
    public static uint NumAssetLibraryInstances { get; private set; }

    /// @see AssetLibrary.as::get url
    public string? Url { get; private set; }

    /// @see AssetLibrary.as::get name
    public string Name => _name ?? "";

    /// @see AssetLibrary.as::get isReady
    public bool IsReady { get; private set; }

    /// @see AssetLibrary.as::get manifest
    public XElement? Manifest => _manifest ??= new XElement("manifest");

    /// @see AssetLibrary.as::get numAssets
    public int NumAssets { get; private set; }

    /// @see AssetLibrary.as::get nameArray
    public IList<string> NameArray => _assetNameArray;

    public bool disposed { get; private set; }

    /// @see AssetLibrary.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        Unload();

        NumAssetLibraryInstances--;
        disposed = true;
        _name = null;
        _manifest = null;
    }

    /// @see AssetLibrary.as::loadFromFile
    /// Godot adaptation: LibraryLoader (Flash SWF loader) not ported. Stores URL for tracking;
    /// real content loading flows through LoadFromResource and the manifest system.
    public void LoadFromFile(object param1, bool param2 = true)
    {
        _contentsExtracted = param2;
        string? url = param1?.ToString();

        if (!string.IsNullOrEmpty(url))
        {
            Url = url;
        }
    }

    /// @see AssetLibrary.as::loadFromResource
    public bool LoadFromResource(XElement param1, Type param2)
    {
        return FetchLibraryContents(this, param1, param2);
    }

    /// @see AssetLibrary.as::unload
    public void Unload()
    {
        foreach (IAsset asset in _assets.Values)
        {
            asset.Dispose();
        }
        _assets.Clear();
        _assetNameArray.Clear();
        NumAssets = 0;
        IsReady = false;
        Url = null;
    }

    /// @see AssetLibrary.as::getAssetByName
    public IAsset? GetAssetByName(string param1)
    {
        return _assets.GetValueOrDefault(param1);
    }

    /// @see AssetLibrary.as::getAssetByIndex
    public IAsset? GetAssetByIndex(int param1)
    {
        if (param1 >= 0 && param1 < _assetNameArray.Count)
        {
            return GetAssetByName(_assetNameArray[param1]);
        }
        return null;
    }

    /// @see AssetLibrary.as::getAssetByContent
    public IAsset? GetAssetByContent(object param1)
    {
        return _assets.Values.FirstOrDefault(asset => ReferenceEquals(asset.Content, param1));
    }

    /// @see AssetLibrary.as::getAssetIndex
    public int GetAssetIndex(IAsset param1)
    {
        foreach (KeyValuePair<string, IAsset> kvp in _assets.Where(kvp => ReferenceEquals(kvp.Value, param1)))
        {
            return _assetNameArray.IndexOf(kvp.Key);
        }

        return -1;
    }

    /// @see AssetLibrary.as::hasAsset
    public bool HasAsset(string param1)
    {
        return _assets.ContainsKey(param1);
    }

    /// @see AssetLibrary.as::setAsset
    public bool SetAsset(string param1, IAsset? param2, bool param3 = true)
    {
        bool isNew = !_assets.ContainsKey(param1);

        if ((!param3 && !isNew) || param2 == null)
        {
            return false;
        }

        if (isNew)
        {
            NumAssets++;
            _assetNameArray.Add(param1);
        }

        _assets[param1] = param2;

        return true;
    }

    /// @see AssetLibrary.as::createAsset
    public IAsset? CreateAsset(string param1, AssetTypeDeclaration? param2)
    {
        if (HasAsset(param1) || param2 == null)
        {
            return null;
        }

        IAsset? asset = CreateAssetInstance(param2.AssetType, param2);

        if (asset == null)
        {
            return null;
        }

        if (SetAsset(param1, asset))
        {
            return asset;
        }

        asset.Dispose();

        return null;
    }

    /// @see AssetLibrary.as::removeAsset
    public IAsset? RemoveAsset(IAsset? param1)
    {
        if (param1 == null)
        {
            return null;
        }

        foreach (KeyValuePair<string, IAsset> kvp in _assets.Where(kvp => ReferenceEquals(kvp.Value, param1)))
        {
            _assetNameArray.Remove(kvp.Key);
            _assets.Remove(kvp.Key);
            NumAssets--;

            return param1;
        }
        return null;
    }

    /// @see AssetLibrary.as::registerAssetTypeDeclaration
    public bool RegisterAssetTypeDeclaration(AssetTypeDeclaration param1, bool param2 = true)
    {
        if (param2)
        {
            if (_sharedTypesByMime!.ContainsKey(param1.MimeType))
            {
                return false;
            }

            _sharedTypesByMime[param1.MimeType] = param1;
        }
        else
        {
            if (!_localTypesByMime.TryAdd(param1.MimeType, param1))
            {
                return false;
            }
        }

        return true;
    }

    /// @see AssetLibrary.as::getAssetTypeDeclarationByMimeType
    public AssetTypeDeclaration? GetAssetTypeDeclarationByMimeType(string param1, bool param2 = true)
    {
        if (param2 && _sharedTypesByMime != null)
        {
            if (_sharedTypesByMime.TryGetValue(param1, out AssetTypeDeclaration? shared))
            {
                return shared;
            }
        }

        _localTypesByMime.TryGetValue(param1, out AssetTypeDeclaration? local);

        return local;
    }

    /// @see AssetLibrary.as::getAssetTypeDeclarationByClass
    public AssetTypeDeclaration? GetAssetTypeDeclarationByClass(Type param1, bool param2 = true)
    {
        if (!param2 || _sharedTypesByMime == null)
        {
            return _localTypesByMime.Values.FirstOrDefault(decl => decl.AssetType == param1);
        }

        foreach (AssetTypeDeclaration decl in _sharedTypesByMime.Values.Where(decl => decl.AssetType == param1))
        {
            return decl;
        }


        return _localTypesByMime.Values.FirstOrDefault(decl => decl.AssetType == param1);
    }

    /// @see AssetLibrary.as::getAssetTypeDeclarationByFileName
    public AssetTypeDeclaration? GetAssetTypeDeclarationByFileName(string param1, bool param2 = true)
    {
        string? ext = ExtractFileExtension(param1);

        if (string.IsNullOrEmpty(ext))
        {
            return null;
        }

        if (!param2 || _sharedTypesByMime == null)
        {
            return _localTypesByMime.Values.FirstOrDefault(decl => Array.IndexOf(decl.FileTypes, ext) >= 0);
        }

        foreach (AssetTypeDeclaration decl in _sharedTypesByMime.Values.Where(decl => Array.IndexOf(decl.FileTypes, ext) >= 0))
        {
            return decl;
        }

        return _localTypesByMime.Values.FirstOrDefault(decl => Array.IndexOf(decl.FileTypes, ext) >= 0);
    }

    /// @see AssetLibrary.as::loadAssetFromFile
    public AssetLoaderStruct? LoadAssetFromFile(string param1, string param2, string? param3 = null, int param4 = -1)
    {
        if (GetAssetByName(param1) != null)
        {
            Logger.Error($"[AssetLibrary] Asset with name {param1} already exists!");
            return null;
        }

        AssetTypeDeclaration? declaration;

        if (param3 != null)
        {
            declaration = GetAssetTypeDeclarationByMimeType(param3);
        }
        else
        {
            declaration = GetAssetTypeDeclarationByFileName(param2);
        }

        if (declaration?.LoaderType == null)
        {
            return null;
        }

        // Godot adaptation: Create the loader WITHOUT the URL to prevent synchronous
        // loading during construction. In AS3, URLLoader.load() is async so event handlers
        // are always attached before completion. Our loaders are synchronous, so we must
        // subscribe handlers first, then trigger the load explicitly.
        if (Activator.CreateInstance(declaration.LoaderType, declaration.MimeType, (string?)null, param4) is not IAssetLoader loader)
        {
            return null;
        }

        AssetLoaderStruct loaderStruct = new(param1, loader);

        loader.LoaderEvent += evt => OnAssetLoadEvent(evt, loader, loaderStruct, declaration);

        // Now trigger the load — handlers are attached and will receive the event
        loader.Load(param2);

        return loaderStruct;
    }

    /// @see AssetLibrary.as::assetLoadEventHandler
    private void OnAssetLoadEvent(AssetLoaderEvent evt, IAssetLoader loader, AssetLoaderStruct loaderStruct,
        AssetTypeDeclaration declaration)
    {
        if (evt.Type != AssetLoaderEvent.ASSET_LOADER_EVENT_COMPLETE)
        {
            return;
        }

        IAsset? asset = CreateAssetInstance(declaration.AssetType, declaration, loader.Url);

        if (asset == null)
        {
            return;
        }

        try
        {
            asset.SetUnknownContent(loader.Content);
            SetAsset(loaderStruct.AssetName!, asset);
        }
        catch
        {
            asset.Dispose();
        }
    }

    /// @see AssetLibrary.as::fetchLibraryContents (static)
    /// Parses manifest XML <library><assets><asset> and <library><aliases><asset ref="..."> elements.
    private static bool FetchLibraryContents(AssetLibrary library, XElement manifest, Type resourceClass)
    {
        XElement? libraryElement = manifest.Element("library")
                                   ?? manifest.Elements().FirstOrDefault(e => e.Name.LocalName == "library");

        if (libraryElement == null)
        {
            // If the manifest itself is the library element
            if (manifest.Name.LocalName == "library")
            {
                libraryElement = manifest;
            }
            else
            {
                return false;
            }
        }

        // Parse <assets><asset> elements
        XElement? assetsElement = libraryElement.Element("assets");
        if (assetsElement != null)
        {
            AssetTypeDeclaration? lastDecl = null;
            string? lastMime = null;

            foreach (XElement assetElement in assetsElement.Elements("asset"))
            {
                string? assetName = assetElement.Attribute("name")?.Value;
                string? mimeType = assetElement.Attribute("mimeType")?.Value;

                if (string.IsNullOrEmpty(assetName))
                {
                    continue;
                }

                AssetTypeDeclaration? decl;
                if (mimeType == lastMime && lastDecl != null)
                {
                    decl = lastDecl;
                }
                else
                {
                    decl = !string.IsNullOrEmpty(mimeType)
                        ? library.GetAssetTypeDeclarationByMimeType(mimeType)
                        : null;
                    lastMime = mimeType;
                    lastDecl = decl;
                }

                if (decl == null)
                {
                    continue;
                }

                IAsset? asset = CreateAssetInstance(decl.AssetType, decl);
                if (asset == null)
                {
                    continue;
                }

                // @see AssetLibrary.as — AS3 uses param2[assetName] to get embedded content.
                // In C# we don't have embedded Class resources — content comes from file loading.
                // For manifest-based loading, assets start with null content (lazy-loaded later).

                IEnumerable<XElement> paramElements = assetElement.Elements("param");
                List<XElement> paramList = paramElements.ToList();
                if (paramList.Count > 0)
                {
                    asset.SetParamsDesc(paramList);
                }

                library.SetAsset(assetName, asset);
            }
        }

        // Parse <aliases><asset ref="..."> elements
        XElement? aliasesElement = libraryElement.Element("aliases");
        if (aliasesElement != null)
        {
            AssetTypeDeclaration? lastDecl = null;
            string? lastMime = null;

            foreach (XElement aliasElement in aliasesElement.Elements("asset"))
            {
                string? aliasName = aliasElement.Attribute("name")?.Value;
                string? mimeType = aliasElement.Attribute("mimeType")?.Value;
                string? refName = aliasElement.Attribute("ref")?.Value;

                if (string.IsNullOrEmpty(aliasName))
                {
                    continue;
                }

                AssetTypeDeclaration? decl;
                if (mimeType == lastMime && lastDecl != null)
                {
                    decl = lastDecl;
                }
                else
                {
                    decl = !string.IsNullOrEmpty(mimeType)
                        ? library.GetAssetTypeDeclarationByMimeType(mimeType)
                        : null;
                    lastMime = mimeType;
                    lastDecl = decl;
                }

                if (decl == null)
                {
                    continue;
                }

                IAsset? asset = CreateAssetInstance(decl.AssetType, decl);

                if (asset == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(refName))
                {
                    IAsset? refAsset = library.GetAssetByName(refName);

                    if (refAsset != null)
                    {
                        asset.SetUnknownContent(refAsset.Content);
                    }
                }

                IEnumerable<XElement> paramElements = aliasElement.Elements("param");
                List<XElement> paramList = paramElements.ToList();

                if (paramList.Count > 0)
                {
                    asset.SetParamsDesc(paramList);
                }

                library.SetAsset(aliasName, asset);
            }
        }

        return true;
    }

    /// Create an IAsset instance from a Type and AssetTypeDeclaration.
    /// @see AssetLibrary.as — `new _loc7_.assetClass(_loc7_) as IAsset`
    private static IAsset? CreateAssetInstance(Type assetType, AssetTypeDeclaration? declaration, string? url = null)
    {
        try
        {
            // Try (AssetTypeDeclaration, string) constructor — matches AS3 pattern
            return Activator.CreateInstance(assetType, declaration, url) as IAsset;
        }
        catch
        {
            try
            {
                // Fallback to parameterless constructor
                return Activator.CreateInstance(assetType) as IAsset;
            }
            catch
            {
                return null;
            }
        }
    }

    private static string? ExtractFileExtension(string fileName)
    {
        int dotIndex = fileName.LastIndexOf('.');

        if (dotIndex < 0)
        {
            return null;
        }

        string ext = fileName[(dotIndex + 1)..];

        int queryIndex = ext.IndexOf('?');

        if (queryIndex > 0)
        {
            ext = ext[..queryIndex];
        }

        return ext.ToLowerInvariant();
    }

    public override string ToString()
    {
        return $"{GetType().FullName}: {_name}";
    }
}
