// @see core/assets/AssetLibraryCollection.as

using System;
using System.Linq;
using System.Xml.Linq;

namespace Vortex.Core.Assets;

/// @see core/assets/AssetLibraryCollection.as
/// Composite pattern: aggregates multiple IAssetLibrary instances into a single facade.
public class AssetLibraryCollection : IAssetLibrary
{
    /// @see AssetLibraryCollection.as::var_21
    protected readonly List<IAssetLibrary> _libraries = [];

    /// @see AssetLibraryCollection.as::var_124 — bin library for direct asset creation
    protected AssetLibrary? _binLibrary;

    /// @see AssetLibraryCollection.as::var_67
    protected XElement? _manifest;

    protected string? _name;

    /// @see AssetLibraryCollection.as::var_1461
    private uint _libCounter;

    /// @see AssetLibraryCollection.as::AssetLibraryCollection
    public AssetLibraryCollection(string name)
    {
        _name = name;
    }

    /// Lazy bin library for setAsset/createAsset operations.
    /// @see AssetLibraryCollection.as::get binLibrary
    private IAssetLibrary BinLibrary
    {
        get
        {
            if (_binLibrary == null)
            {
                _binLibrary = new AssetLibrary("bin");
                _libraries.Insert(0, _binLibrary);
            }
            return _binLibrary;
        }
    }

    /// @see AssetLibraryCollection.as::get url
    public string? Url => "";

    /// @see AssetLibraryCollection.as::get name
    public string Name => _name ?? "";

    /// @see AssetLibraryCollection.as::get isReady
    public bool IsReady => true;

    /// @see AssetLibraryCollection.as::get numAssets
    public int NumAssets => _libraries.Sum(l => l.NumAssets);

    /// @see AssetLibraryCollection.as::get manifest
    public XElement? Manifest => _manifest ??= new XElement("manifest");

    /// @see AssetLibraryCollection.as::getManifests
    public List<XElement> GetManifests()
    {
        List<XElement> result = new();

        foreach (IAssetLibrary lib in _libraries)
        {
            if (lib.Manifest != null)
            {
                result.Add(lib.Manifest);
            }
        }
        return result;
    }

    /// @see AssetLibraryCollection.as::get nameArray
    public IList<string> NameArray
    {
        get
        {
            List<string> result = new();
            foreach (IAssetLibrary lib in _libraries)
            {
                result.AddRange(lib.NameArray);
            }
            return result;
        }
    }

    public bool disposed { get; private set; }

    /// @see AssetLibraryCollection.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        while (_libraries.Count > 0)
        {
            IAssetLibrary lib = _libraries[^1];
            _libraries.RemoveAt(_libraries.Count - 1);
            lib.Dispose();
        }

        _binLibrary = null;
    }

    /// @see AssetLibraryCollection.as::loadFromFile
    /// Godot adaptation: LibraryLoader not ported. Delegates to a new AssetLibrary.
    public void LoadFromFile(object param1, bool param2 = true)
    {
        AssetLibrary lib = new("lib-" + _libCounter++);
        lib.LoadFromFile(param1, param2);
        _libraries.Add(lib);
    }

    /// @see AssetLibraryCollection.as::loadFromResource
    public bool LoadFromResource(XElement param1, Type param2)
    {
        return BinLibrary.LoadFromResource(param1, param2);
    }

    /// @see AssetLibraryCollection.as::unload
    public void Unload()
    {
        foreach (IAssetLibrary lib in _libraries.ToArray())
        {
            lib.Dispose();
        }
        _libraries.Clear();
        _binLibrary = null;
    }

    /// @see AssetLibraryCollection.as::getAssetByName
    /// Godot adaptation: In AS3, manifest-loaded libraries had real content from embedded Flash
    /// resources. In C#, manifest libraries create placeholder assets with null Content (lazy).
    /// We skip null-content assets and continue searching so bundle-loaded libraries with real
    /// content are found. If no asset with content exists, return the first null-content match.
    public IAsset? GetAssetByName(string param1)
    {
        IAsset? nullContentFallback = null;

        foreach (IAssetLibrary lib in _libraries)
        {
            IAsset? asset = lib.GetAssetByName(param1);
            if (asset != null)
            {
                if (asset.Content != null)
                {
                    return asset;
                }

                nullContentFallback ??= asset;
            }
        }
        return nullContentFallback;
    }

    /// @see AssetLibraryCollection.as::getAssetByIndex
    public IAsset? GetAssetByIndex(int param1)
    {
        int offset = 0;
        foreach (IAssetLibrary lib in _libraries)
        {
            if (param1 < offset + lib.NumAssets)
            {
                return lib.GetAssetByIndex(param1 - offset);
            }
            offset += lib.NumAssets;
        }
        return null;
    }

    /// @see AssetLibraryCollection.as::getAssetByContent
    public IAsset? GetAssetByContent(object param1)
    {
        foreach (IAssetLibrary lib in _libraries)
        {
            IAsset? asset = lib.GetAssetByContent(param1);
            if (asset != null)
            {
                return asset;
            }
        }
        return null;
    }

    /// @see AssetLibraryCollection.as::getAssetIndex
    public int GetAssetIndex(IAsset param1)
    {
        int offset = 0;
        foreach (IAssetLibrary lib in _libraries)
        {
            int index = lib.GetAssetIndex(param1);
            if (index >= 0)
            {
                return offset + index;
            }
            offset += lib.NumAssets;
        }
        return -1;
    }

    /// @see AssetLibraryCollection.as::hasAsset
    public bool HasAsset(string param1)
    {
        foreach (IAssetLibrary lib in _libraries)
        {
            if (lib.HasAsset(param1))
            {
                return true;
            }
        }
        return false;
    }

    /// @see AssetLibraryCollection.as::setAsset
    public bool SetAsset(string param1, IAsset param2, bool param3 = true)
    {
        return BinLibrary.SetAsset(param1, param2, param3);
    }

    /// @see AssetLibraryCollection.as::createAsset
    public IAsset? CreateAsset(string param1, AssetTypeDeclaration param2)
    {
        return BinLibrary.CreateAsset(param1, param2);
    }

    /// @see AssetLibraryCollection.as::removeAsset
    public IAsset? RemoveAsset(IAsset param1)
    {
        foreach (IAssetLibrary lib in _libraries)
        {
            if (ReferenceEquals(lib.RemoveAsset(param1), param1))
            {
                return param1;
            }
        }
        return null;
    }

    /// @see AssetLibraryCollection.as::registerAssetTypeDeclaration
    public bool RegisterAssetTypeDeclaration(AssetTypeDeclaration param1, bool param2 = true)
    {
        return BinLibrary.RegisterAssetTypeDeclaration(param1, param2);
    }

    /// @see AssetLibraryCollection.as::getAssetTypeDeclarationByMimeType
    public AssetTypeDeclaration? GetAssetTypeDeclarationByMimeType(string param1, bool param2 = true)
    {
        if (param2)
        {
            return BinLibrary.GetAssetTypeDeclarationByMimeType(param1, true);
        }
        foreach (IAssetLibrary lib in _libraries)
        {
            AssetTypeDeclaration? decl = lib.GetAssetTypeDeclarationByMimeType(param1, false);
            if (decl != null)
            {
                return decl;
            }
        }
        return null;
    }

    /// @see AssetLibraryCollection.as::getAssetTypeDeclarationByClass
    public AssetTypeDeclaration? GetAssetTypeDeclarationByClass(Type param1, bool param2 = true)
    {
        if (param2)
        {
            return BinLibrary.GetAssetTypeDeclarationByClass(param1, true);
        }
        foreach (IAssetLibrary lib in _libraries)
        {
            AssetTypeDeclaration? decl = lib.GetAssetTypeDeclarationByClass(param1, false);
            if (decl != null)
            {
                return decl;
            }
        }
        return null;
    }

    /// @see AssetLibraryCollection.as::getAssetTypeDeclarationByFileName
    public AssetTypeDeclaration? GetAssetTypeDeclarationByFileName(string param1, bool param2 = true)
    {
        if (param2)
        {
            return BinLibrary.GetAssetTypeDeclarationByFileName(param1, true);
        }
        foreach (IAssetLibrary lib in _libraries)
        {
            AssetTypeDeclaration? decl = lib.GetAssetTypeDeclarationByFileName(param1, false);
            if (decl != null)
            {
                return decl;
            }
        }
        return null;
    }

    /// @see AssetLibraryCollection.as::loadAssetFromFile
    public AssetLoaderStruct? LoadAssetFromFile(string param1, string param2, string? param3 = null, int param4 = -1)
    {
        return BinLibrary.LoadAssetFromFile(param1, param2, param3, param4);
    }

    /// @see AssetLibraryCollection.as::hasAssetLibrary
    public bool HasAssetLibrary(string param1)
    {
        return _libraries.Any(l => l.Name == param1);
    }

    /// @see AssetLibraryCollection.as::getAssetLibraryByName
    public IAssetLibrary? GetAssetLibraryByName(string param1)
    {
        return _libraries.FirstOrDefault(l => l.Name == param1);
    }

    /// @see AssetLibraryCollection.as::getAssetLibraryByUrl
    public IAssetLibrary? GetAssetLibraryByUrl(string param1)
    {
        return _libraries.FirstOrDefault(l => l.Url == param1);
    }

    /// @see AssetLibraryCollection.as::getAssetLibraryByPartialUrl
    public IAssetLibrary? GetAssetLibraryByPartialUrl(string param1)
    {
        return _libraries.FirstOrDefault(l => l.Url != null && l.Url.Contains(param1, StringComparison.Ordinal));
    }

    /// @see AssetLibraryCollection.as::addAssetLibrary
    public void AddAssetLibrary(IAssetLibrary param1)
    {
        if (!_libraries.Contains(param1))
        {
            _libraries.Add(param1);
        }
    }

    /// @see AssetLibraryCollection.as::removeAssetLibrary
    public void RemoveAssetLibrary(IAssetLibrary param1)
    {
        _libraries.Remove(param1);
    }
}
