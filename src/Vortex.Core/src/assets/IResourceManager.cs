// @see core/assets/IResourceManager.as

using System;

namespace Vortex.Core.Assets;

/// @see core/assets/IResourceManager.as
public interface IResourceManager : IDisposable
{
    /// @see IResourceManager.as::createAsset
    IAsset? CreateAsset(string param1, Type param2, object? param3);

    void RetrieveAsset(string assetUri, IAssetReceiver receiver);

    bool IsSameAsset(string assetUri, string resolvedName);

    void RemoveAsset(string assetUri);
}
