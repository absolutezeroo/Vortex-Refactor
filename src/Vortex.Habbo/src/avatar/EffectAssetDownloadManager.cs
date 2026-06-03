// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/EffectAssetDownloadManager.as

using System.Linq;
using System.Xml.Linq;

using Vortex.Core.Assets;
using Vortex.Core.Assets.Loaders;
using Vortex.Core.Runtime.Events;
using Vortex.Habbo.Avatar.Events;

namespace Vortex.Habbo.Avatar;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/EffectAssetDownloadManager.as
public class EffectAssetDownloadManager
{
    public const string LIBRARY_LOADED = "LIBRARY_LOADED";

    private const int DOWNLOAD_TIMEOUT = 100;
    private const int MAX_SIMULTANEOUS_DOWNLOADS = 2;

    private readonly List<string> _mandatoryEffects;
    private readonly IAssetLibrary _assets;
    private readonly string _effectMapUrl;
    private readonly string _downloadUrl;
    private readonly string _urlTemplate;
    private readonly AvatarStructure _structure;
    private readonly Dictionary<string, List<IAvatarEffectListener>> _listeners;
    private readonly Dictionary<string, List<EffectAssetDownloadLibrary>> _pendingEffects;
    private readonly List<object[]> _initBuffer;
    private readonly List<EffectAssetDownloadLibrary> _pendingQueue;
    private readonly List<EffectAssetDownloadLibrary> _activeDownloads;
    private bool _disposed;

    public EventDispatcherWrapper Events { get; } = new();

    /// @see EffectAssetDownloadManager.as::EffectAssetDownloadManager
    public EffectAssetDownloadManager
    (
        IAssetLibrary assets,
        string effectMapUrl,
        string downloadUrl,
        AvatarStructure structure,
        string urlTemplate
    )
    {
        _mandatoryEffects = ["dance.1", "dance.2", "dance.3", "dance.4"];
        EffectMap = new Dictionary<string, List<EffectAssetDownloadLibrary>>();
        _assets = assets;
        _structure = structure;
        _effectMapUrl = effectMapUrl;
        _downloadUrl = downloadUrl;
        _urlTemplate = urlTemplate;
        _listeners = new Dictionary<string, List<IAvatarEffectListener>>();
        _pendingEffects = new Dictionary<string, List<EffectAssetDownloadLibrary>>();
        _initBuffer = [];
        _pendingQueue = [];
        _activeDownloads = [];

        _structure.RenderManager.Events.AddEventListener(AvatarRenderEvent.AVATAR_RENDER_READY, PurgeInitDownloadBuffer);

        // Try to load effectmap from existing assets
        IAsset? existingAsset = _assets.GetAssetByName("effectmap");
        if (existingAsset?.Content is XElement xml)
        {
            LoadEffectMapData(new XElement(xml));
        }
        else
        {
            // Attempt to load effectmap from file path
            try
            {
                BinaryFileLoader loader = new("text/xml", effectMapUrl);

                if (loader.Bytes is { Length: > 0 })
                {
                    string text = System.Text.Encoding.UTF8.GetString(loader.Bytes);
                    LoadEffectMapData(XElement.Parse(text));
                }
                else
                {
                    Logger.Warn("[EffectAssetDownloadManager] effectmap not found at " + effectMapUrl);
                    IsMapReady = true;
                    Events.DispatchEvent("complete");
                }

                loader.Dispose();
            }
            catch (System.Exception e)
            {
                Logger.Warn("[EffectAssetDownloadManager] effectmap load failed: " + e.Message);
                IsMapReady = true;
                Events.DispatchEvent("complete");
            }
        }
    }

    /// @see EffectAssetDownloadManager.as::loadMandatoryLibs
    public void LoadMandatoryLibs()
    {
        List<string> mandatoryCopy = new(_mandatoryEffects);

        foreach (string effectId in mandatoryCopy)
        {
            if (!EffectMap.TryGetValue(effectId, out List<EffectAssetDownloadLibrary>? libs))
            {
                continue;
            }

            foreach (EffectAssetDownloadLibrary lib in libs)
            {
                AddToQueue(lib);
            }
        }
    }

    /// @see EffectAssetDownloadManager.as::loadEffectMapData
    private void LoadEffectMapData(XElement? xml)
    {
        if (xml == null || string.IsNullOrEmpty(xml.ToString()))
        {
            return;
        }

        GenerateMap(xml);
        LoadMandatoryLibs();

        IsMapReady = true;

        Events.DispatchEvent("complete");
    }

    /// @see EffectAssetDownloadManager.as::generateMap
    private void GenerateMap(XElement xml)
    {
        foreach (XElement effectXml in xml.Elements("effect"))
        {
            string libName = (string?)effectXml.Attribute("lib") ?? "";
            string effectId = (string?)effectXml.Attribute("id") ?? "";

            EffectAssetDownloadLibrary lib = new(libName, "0", _downloadUrl, _assets, _urlTemplate);

            lib.Events.AddEventListener("complete", _ => LibraryComplete(lib));

            if (!EffectMap.TryGetValue(effectId, out List<EffectAssetDownloadLibrary>? list))
            {
                list = [];
                EffectMap[effectId] = list;
            }

            list.Add(lib);
        }
    }

    /// @see EffectAssetDownloadManager.as::libraryComplete
    private void LibraryComplete(EffectAssetDownloadLibrary completedLib)
    {
        if (_disposed)
        {
            return;
        }

        // Register animation if available
        if (completedLib.Animation != null)
        {
            _structure.RegisterAnimation(completedLib.Animation);
        }

        List<string> completedEffects = new();

        foreach ((string effectId, List<EffectAssetDownloadLibrary> libs) in _pendingEffects)
        {
            bool allReady = libs.All(lib => lib.IsReady);

            if (!allReady)
            {
                continue;
            }

            completedEffects.Add(effectId);

            if (!_listeners.TryGetValue(effectId, out List<IAvatarEffectListener>? listenerList))
            {
                continue;
            }

            foreach (IAvatarEffectListener listener in listenerList)
            {
                if (listener is { disposed: false })
                {
                    listener.AvatarEffectReady(int.Parse(effectId));
                }
            }

            _listeners.Remove(effectId);
        }

        foreach (string effectId in completedEffects)
        {
            _pendingEffects.Remove(effectId);
        }

        // Remove from active downloads
        for (int i = 0;
             i < _activeDownloads.Count;
             i++)
        {
            if (_activeDownloads[i].Name != completedLib.Name)
            {
                continue;
            }

            _activeDownloads.RemoveAt(i);

            break;
        }

        if (completedEffects.Count > 0)
        {
            Events.DispatchEvent(LIBRARY_LOADED, new LibraryLoadedEvent(LIBRARY_LOADED, completedLib.Name));
        }

        ProcessPending();
    }

    /// @see EffectAssetDownloadManager.as::isReady
    public bool IsReady(int effectId)
    {
        if (!IsMapReady || !_structure.RenderManager.IsReady)
        {
            return false;
        }

        List<EffectAssetDownloadLibrary> libs = GetLibsToDownload(effectId);

        return libs.Count == 0;
    }

    /// @see EffectAssetDownloadManager.as::loadEffectData
    public void LoadEffectData(int effectId, IAvatarEffectListener? listener)
    {
        if (!IsMapReady || !_structure.RenderManager.IsReady)
        {
            _initBuffer.Add([effectId, listener!]);

            return;
        }

        List<EffectAssetDownloadLibrary> libs = GetLibsToDownload(effectId);
        string key = effectId.ToString();

        if (libs.Count > 0)
        {
            if (listener is { disposed: false })
            {
                if (!_listeners.TryGetValue(key, out List<IAvatarEffectListener>? listenerList))
                {
                    listenerList = [];
                    _listeners[key] = listenerList;
                }

                listenerList.Add(listener);
            }

            _pendingEffects[key] = libs;

            foreach (EffectAssetDownloadLibrary lib in libs)
            {
                AddToQueue(lib);
            }
        }
        else if (listener is { disposed: false })
        {
            listener.AvatarEffectReady(effectId);
        }
    }

    /// @see EffectAssetDownloadManager.as::getLibsToDownload
    private List<EffectAssetDownloadLibrary> GetLibsToDownload(int effectId)
    {
        List<EffectAssetDownloadLibrary> result = new();

        if (_structure == null || !EffectMap.TryGetValue(effectId.ToString(), out List<EffectAssetDownloadLibrary>? libs))
        {
            return result;
        }

        foreach (EffectAssetDownloadLibrary lib in libs.OfType<EffectAssetDownloadLibrary>()
                                                       .Where(lib => !lib.IsReady && !result.Contains(lib)))
        {
            result.Add(lib);
        }

        return result;
    }

    /// @see EffectAssetDownloadManager.as::processPending
    private void ProcessPending()
    {
        while (_pendingQueue.Count > 0 && _activeDownloads.Count < MAX_SIMULTANEOUS_DOWNLOADS)
        {
            EffectAssetDownloadLibrary lib = _pendingQueue[0];

            _pendingQueue.RemoveAt(0);
            lib.StartDownloading();
            _activeDownloads.Add(lib);
        }
    }

    /// @see EffectAssetDownloadManager.as::addToQueue
    private void AddToQueue(EffectAssetDownloadLibrary lib)
    {
        if (lib.IsReady || _pendingQueue.Contains(lib) || _activeDownloads.Contains(lib))
        {
            return;
        }

        _pendingQueue.Add(lib);

        ProcessPending();
    }

    /// @see EffectAssetDownloadManager.as::purgeInitDownloadBuffer
    private void PurgeInitDownloadBuffer(object? param)
    {
        foreach (object[] entry in _initBuffer)
        {
            LoadEffectData((int)entry[0], entry[1] as IAvatarEffectListener);
        }

        _initBuffer.Clear();
    }

    /// True if the effect map was already loaded (possibly synchronously during construction).
    public bool IsMapReady { get; private set; }

    /// @see EffectAssetDownloadManager.as::get map
    public Dictionary<string, List<EffectAssetDownloadLibrary>> EffectMap { get; }

    /// @see EffectAssetDownloadManager.as::dispose
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Events.Dispose();
        _listeners.Clear();
        _pendingEffects.Clear();
        _pendingQueue.Clear();
        _initBuffer.Clear();
    }
}
