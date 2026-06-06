// @see WIN63-202407091256-704579380-Source-main/habbo/window/ResourceManager.as

using System;

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Assets.Loaders;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Window;

/// @see WIN63-202407091256-704579380-Source-main/habbo/window/ResourceManager.as
public class ResourceManager : IResourceManager
{
    private HabboWindowManagerComponent? _windowManager;

    /// @see ResourceManager.as::_assetReceivers
    private readonly Dictionary<string, List<IAssetReceiver>> _assetReceivers = new(StringComparer.Ordinal);

    /// @see ResourceManager.as::ResourceManager
    public ResourceManager() { }

    /// @see ResourceManager.as::ResourceManager
    public ResourceManager(HabboWindowManagerComponent? param1)
    {
        _windowManager = param1;
    }

    /// @see ResourceManager.as::get disposed
    public bool disposed { get; private set; }

    /// @see ResourceManager.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        _windowManager = null;
        _assetReceivers.Clear();
        disposed = true;
    }

    /// @see ResourceManager.as::createAsset
    public IAsset? CreateAsset(string param1, Type param2, object? param3)
    {
        if (_windowManager?.assets is not IAssetLibrary library)
        {
            return null;
        }

        AssetTypeDeclaration? declaration = library.GetAssetTypeDeclarationByClass(param2);

        if (declaration == null)
        {
            return null;
        }

        IAsset? asset = Activator.CreateInstance(param2, declaration, null) as IAsset;

        if (asset == null)
        {
            return null;
        }

        library.SetAsset(param1, asset);
        asset.SetUnknownContent(param3);

        return asset;
    }

    /// @see ResourceManager.as::retrieveAsset
    public void RetrieveAsset(string assetUri, IAssetReceiver receiver)
    {
        if (string.IsNullOrEmpty(assetUri))
        {
            return;
        }

        string? resolvedName = ResolveAssetName(assetUri);

        if (resolvedName == null)
        {
            return;
        }

        if (_windowManager?.assets is not IAssetLibrary library)
        {
            // Godot adaptation: asset library not loaded (bootstrap passes null); fall back to filesystem.
            // Consistent with HabboWindowManagerComponent.EnsureInitialized filesystem fallback pattern.
            Image? fallbackImage = HabboAssetResolver.LoadImageAsset(resolvedName);

            if (fallbackImage != null)
            {
                BitmapDataAsset fallbackAsset = new(null);
                fallbackAsset.SetUnknownContent(fallbackImage);
                receiver.ReceiveAsset(fallbackAsset, resolvedName);
            }

            return;
        }

        IAsset? asset = library.GetAssetByName(resolvedName);

        if (asset == null)
        {
            if (IsHttpUrl(resolvedName))
            {
                try
                {
                    AssetLoaderStruct? loader = library.LoadAssetFromFile(resolvedName, resolvedName);

                    if (loader != null && !loader.disposed)
                    {
                        if (!_assetReceivers.TryGetValue(resolvedName, out List<IAssetReceiver>? receivers))
                        {
                            receivers = [];
                            _assetReceivers[resolvedName] = receivers;
                        }

                        receivers.Add(receiver);
                        loader.LoaderEvent += evt =>
                        {
                            if (evt.Type == AssetLoaderEvent.ASSET_LOADER_EVENT_COMPLETE)
                            {
                                PassAssetToCallback(loader);
                            }
                        };

                        if (library.GetAssetByName(resolvedName) != null)
                        {
                            PassAssetToCallback(loader);
                        }
                    }
                }
                catch
                {
                    PassMissingImageToCallback(library, receiver, resolvedName);
                }
            }

            return;
        }

        receiver.ReceiveAsset(asset, resolvedName);
    }

    /// @see ResourceManager.as::passAssetToCallback
    private void PassAssetToCallback(AssetLoaderStruct loader)
    {
        if (disposed || loader.AssetName == null || _windowManager?.assets is not IAssetLibrary library)
        {
            return;
        }

        if (!_assetReceivers.TryGetValue(loader.AssetName, out List<IAssetReceiver>? receivers))
        {
            return;
        }

        IAsset? asset = library.GetAssetByName(loader.AssetName);

        if (asset == null)
        {
            return;
        }

        foreach (IAssetReceiver receiver in receivers)
        {
            if (!receiver.disposed)
            {
                receiver.ReceiveAsset(asset, asset.Url ?? loader.AssetName);
            }
        }

        _assetReceivers.Remove(loader.AssetName);
    }

    /// @see ResourceManager.as::isSameAsset
    public bool IsSameAsset(string assetUri, string resolvedName)
    {
        return resolvedName == ResolveAssetName(assetUri);
    }

    /// @see ResourceManager.as::resolveAssetName
    private string? ResolveAssetName(string param1)
    {
        return _windowManager?.Interpolate(param1);
    }

    /// @see ResourceManager.as::removeAsset
    public void RemoveAsset(string param1)
    {
        string? resolved = ResolveAssetName(param1);

        if (resolved == null || _windowManager?.assets is not IAssetLibrary library)
        {
            return;
        }

        IAsset? asset = library.GetAssetByName(resolved);

        if (asset != null)
        {
            library.RemoveAsset(asset);
        }
    }

    private static bool IsHttpUrl(string value)
    {
        return value.StartsWith("http://", StringComparison.Ordinal) ||
               value.StartsWith("https://", StringComparison.Ordinal);
    }

    private static void PassMissingImageToCallback(IAssetLibrary library, IAssetReceiver receiver, string resolvedName)
    {
        if (library.GetAssetByName("missing_image_icon") is not BitmapDataAsset missingAsset ||
            missingAsset.Content is not Image missingImage)
        {
            return;
        }

        Image image = Image.CreateFromData(
            missingImage.GetWidth(),
            missingImage.GetHeight(),
            missingImage.HasMipmaps(),
            missingImage.GetFormat(),
            missingImage.GetData()
        );

        BitmapDataAsset asset = new(null);
        asset.SetUnknownContent(image);
        receiver.ReceiveAsset(asset, resolvedName);
    }
}
