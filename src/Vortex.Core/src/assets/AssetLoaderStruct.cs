// @see core/assets/AssetLoaderStruct.as

using System;

using Vortex.Core.Assets.Loaders;

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Core.Assets;

/// @see core/assets/AssetLoaderStruct.as
/// Wrapper combining an asset name with its loader instance.
public class AssetLoaderStruct : IDisposable
{
    /// @see AssetLoaderStruct.as::AssetLoaderStruct
    public AssetLoaderStruct(string assetName, IAssetLoader loader)
    {
        AssetName = assetName;
        AssetLoader = loader;
    }

    /// @see AssetLoaderStruct.as::get assetName
    public string? AssetName { get; private set; }

    /// @see AssetLoaderStruct.as::get assetLoader
    public IAssetLoader? AssetLoader { get; private set; }

    public bool disposed { get; private set; }

    /// Event passthrough from the underlying loader.
    public event Action<AssetLoaderEvent>? LoaderEvent
    {
        add
        {
            if (AssetLoader != null)
            {
                AssetLoader.LoaderEvent += value;
            }
        }
        remove
        {
            if (AssetLoader != null)
            {
                AssetLoader.LoaderEvent -= value;
            }
        }
    }

    /// @see AssetLoaderStruct.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (AssetLoader is
            {
                disposed: false,
            })
        {
            AssetLoader.Dispose();
            AssetLoader = null;
        }

        AssetName = null;
        disposed = true;
    }
}
