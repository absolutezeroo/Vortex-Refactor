// @see WIN63-202407091256-704579380-Source-main/habbo/window/ResourceManager.as

using System;

using Godot;

using Vortex.Core.Assets;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Window;

/// @see WIN63-202407091256-704579380-Source-main/habbo/window/ResourceManager.as
public class ResourceManager : IResourceManager
{
    private HabboWindowManagerComponent? _windowManager;

    /// Asset cache: resolved name → loaded asset.
    /// @see ResourceManager.as — AS3 uses _windowManager.assets (IAssetLibrary).
    private readonly Dictionary<string, IAsset?> _assetCache = new(StringComparer.Ordinal);

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
        _assetCache.Clear();
        disposed = true;
    }

    /// @see IResourceManager.as::createAsset
    public IAsset? CreateAsset(string param1, Type param2, object? param3)
    {
        // @see ResourceManager.as — delegates to _windowManager.assets as IAssetLibrary
        if (_windowManager?.assets is not IAssetLibrary assetLibrary)
        {
            return null;
        }

        AssetTypeDeclaration? declaration = assetLibrary.GetAssetTypeDeclarationByClass(param2);

        if (declaration == null)
        {
            return null;
        }

        IAsset? asset = assetLibrary.CreateAsset(param1, declaration);

        if (asset != null && param3 != null)
        {
            asset.SetUnknownContent(param3);
        }

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

        // @see ResourceManager.as — check asset cache first
        if (_assetCache.TryGetValue(resolvedName, out IAsset? cachedAsset))
        {
            if (cachedAsset != null)
            {
                receiver.ReceiveAsset(cachedAsset, resolvedName);
            }

            return;
        }

        // @see ResourceManager.as — AS3 checks _windowManager.assets.getAssetByName()
        if (_windowManager?.assets is IAssetLibrary library)
        {
            IAsset? libraryAsset = library.GetAssetByName(resolvedName);

            if (libraryAsset != null)
            {
                _assetCache[resolvedName] = libraryAsset;

                receiver.ReceiveAsset(libraryAsset, resolvedName);

                return;
            }
        }

        // Godot adaptation: fall back to filesystem via HabboAssetResolver.
        Image? image = HabboAssetResolver.LoadImageAsset(resolvedName);

        if (image != null)
        {
            BitmapDataAsset asset = new(image)
            {
                Url = resolvedName,
            };
            _assetCache[resolvedName] = asset;
            receiver.ReceiveAsset(asset, resolvedName);
        }
        else
        {
            // @see ResourceManager.as — AS3 uses "missing_image_icon" as fallback
            _assetCache[resolvedName] = null;
        }
    }

    /// @see ResourceManager.as::isSameAsset
    public bool IsSameAsset(string assetUri, string resolvedName)
    {
        return resolvedName == ResolveAssetName(assetUri);
    }

    /// @see ResourceManager.as::resolveAssetName
    private string? ResolveAssetName(string param1)
    {
        return _windowManager?.context.configuration?.Interpolate(param1) ?? param1;
    }

    /// @see ResourceManager.as::removeAsset
    public void RemoveAsset(string param1)
    {
        string? resolved = ResolveAssetName(param1);

        if (resolved != null)
        {
            _assetCache.Remove(resolved);
        }
    }
}
