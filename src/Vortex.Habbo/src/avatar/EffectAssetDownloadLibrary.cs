// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/EffectAssetDownloadLibrary.as

using System.Xml.Linq;

using Vortex.Core.Assets;
using Vortex.Core.Assets.Loaders;
using Vortex.Core.Runtime.Events;

namespace Vortex.Habbo.Avatar;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/EffectAssetDownloadLibrary.as
public class EffectAssetDownloadLibrary
{
    private static readonly int STATE_IDLE = 0;
    private static readonly int STATE_DOWNLOADING = 1;
    private static readonly int STATE_READY = 2;

    private int _state;
    private readonly string _downloadUrl;
    private readonly IAssetLibrary _assets;

    public EventDispatcherWrapper Events { get; } = new();

    /// @see EffectAssetDownloadLibrary.as::EffectAssetDownloadLibrary
    public EffectAssetDownloadLibrary(string name, string revision, string downloadUrl, IAssetLibrary assets, string urlTemplate)
    {
        _state = STATE_IDLE;
        _assets = assets;
        Name = name;
        _downloadUrl = (downloadUrl + urlTemplate)
                       .Replace("%libname%", name)
                       .Replace("%revision%", revision);

        // Check if library already loaded
        if (assets is not AssetLibraryCollection collection)
        {
            return;
        }

        IAssetLibrary? existing = collection.GetAssetLibraryByUrl(name + ".swf");

        if (existing != null)
        {
            _state = STATE_READY;
        }
    }

    /// @see EffectAssetDownloadLibrary.as::dispose
    public void Dispose()
    {
        Events.Dispose();
    }

    /// @see EffectAssetDownloadLibrary.as::startDownloading
    /// Godot adaptation: loads .vortex bundle from local path instead of HTTP download.
    public void StartDownloading()
    {
        _state = STATE_DOWNLOADING;

        try
        {
            VortexBundleFileLoader loader = new(VortexBundleAsset.MIME_TYPE, _downloadUrl);

            if (loader.Content == null)
            {
                Logger.Warn("[EffectAssetDownloadLibrary] Bundle not found: " + _downloadUrl + ", marking ready (graceful)");
                loader.Dispose();
                _state = STATE_READY;
                Events.DispatchEvent("complete");
                return;
            }

            AssetLibrary lib = new(Name);
            VortexBundleAsset bundleAsset = new(url: Name);
            bundleAsset.SetUnknownContent(loader.Content);
            bundleAsset.PopulateLibrary(lib);

            // Extract animation XML from the bundle's manifest if present
            IAsset? animAsset = lib.GetAssetByName("animation");
            if (animAsset?.Content is XElement animXml)
            {
                Animation = animXml;
            }

            if (_assets is AssetLibraryCollection collection)
            {
                collection.AddAssetLibrary(lib);
            }

            loader.Dispose();
        }
        catch (System.Exception e)
        {
            Logger.Warn("[EffectAssetDownloadLibrary] Failed to load bundle " + _downloadUrl + ": " + e.Message);
        }

        _state = STATE_READY;
        Events.DispatchEvent("complete");
    }

    /// @see EffectAssetDownloadLibrary.as::get name
    public string Name { get; }

    /// @see EffectAssetDownloadLibrary.as::get isReady
    public bool IsReady => _state == STATE_READY;

    /// @see EffectAssetDownloadLibrary.as::get animation
    public XElement? Animation { get; private set; }

    /// @see EffectAssetDownloadLibrary.as::toString
    public override string ToString()
    {
        return Name + (IsReady ? "[x]" : "[ ]");
    }
}
