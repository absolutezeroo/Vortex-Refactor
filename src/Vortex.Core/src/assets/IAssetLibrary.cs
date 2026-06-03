// @see core/assets/IAssetLibrary.as

using System;
using System.Xml.Linq;

namespace Vortex.Core.Assets;

/// @see core/assets/IAssetLibrary.as
/// Full asset library interface: storage, retrieval, type registration, and loading.
public interface IAssetLibrary : IDisposable
{
    /// @see IAssetLibrary.as::get url
    string? Url { get; }

    /// @see IAssetLibrary.as::get name
    string Name { get; }

    /// @see IAssetLibrary.as::get isReady
    bool IsReady { get; }

    /// @see IAssetLibrary.as::get numAssets
    int NumAssets { get; }

    /// @see IAssetLibrary.as::get manifest
    XElement? Manifest { get; }

    /// @see IAssetLibrary.as::get nameArray
    IList<string> NameArray { get; }

    /// @see IAssetLibrary.as::getAssetByName
    IAsset? GetAssetByName(string param1);

    /// @see IAssetLibrary.as::getAssetByIndex
    IAsset? GetAssetByIndex(int param1);

    /// @see IAssetLibrary.as::getAssetByContent
    IAsset? GetAssetByContent(object param1);

    /// @see IAssetLibrary.as::getAssetIndex
    int GetAssetIndex(IAsset param1);

    /// @see IAssetLibrary.as::hasAsset
    bool HasAsset(string param1);

    /// @see IAssetLibrary.as::setAsset
    bool SetAsset(string param1, IAsset param2, bool param3 = true);

    /// @see IAssetLibrary.as::createAsset
    IAsset? CreateAsset(string param1, AssetTypeDeclaration param2);

    /// @see IAssetLibrary.as::removeAsset
    IAsset? RemoveAsset(IAsset param1);

    /// @see IAssetLibrary.as::registerAssetTypeDeclaration
    bool RegisterAssetTypeDeclaration(AssetTypeDeclaration param1, bool param2 = true);

    /// @see IAssetLibrary.as::getAssetTypeDeclarationByMimeType
    AssetTypeDeclaration? GetAssetTypeDeclarationByMimeType(string param1, bool param2 = true);

    /// @see IAssetLibrary.as::getAssetTypeDeclarationByClass
    AssetTypeDeclaration? GetAssetTypeDeclarationByClass(Type param1, bool param2 = true);

    /// @see IAssetLibrary.as::getAssetTypeDeclarationByFileName
    AssetTypeDeclaration? GetAssetTypeDeclarationByFileName(string param1, bool param2 = true);

    /// @see IAssetLibrary.as::loadFromFile
    void LoadFromFile(object param1, bool param2 = true);

    /// @see IAssetLibrary.as::loadAssetFromFile
    AssetLoaderStruct? LoadAssetFromFile(string param1, string param2, string? param3 = null, int param4 = -1);

    /// @see IAssetLibrary.as::loadFromResource
    bool LoadFromResource(XElement param1, Type param2);

    /// @see IAssetLibrary.as::unload
    void Unload();
}
