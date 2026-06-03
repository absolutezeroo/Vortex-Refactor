// @see core/assets/AssetTypeDeclaration.as

using System;

namespace Vortex.Core.Assets;

/// @see core/assets/AssetTypeDeclaration.as
/// Metadata describing an asset type: its MIME type, implementation class, loader class, and file extensions.
public class AssetTypeDeclaration
{
    /// @see AssetTypeDeclaration.as::mimeType
    public string MimeType { get; }

    /// @see AssetTypeDeclaration.as::assetClass
    public Type AssetType { get; }

    /// @see AssetTypeDeclaration.as::loaderClass
    public Type? LoaderType { get; }

    /// @see AssetTypeDeclaration.as::fileTypes
    public string[] FileTypes { get; }

    /// @see AssetTypeDeclaration.as::AssetTypeDeclaration
    public AssetTypeDeclaration(string mimeType, Type assetType, Type? loaderType = null, params string[] fileTypes)
    {
        MimeType = mimeType;
        AssetType = assetType;
        LoaderType = loaderType;
        FileTypes = fileTypes;
    }
}
