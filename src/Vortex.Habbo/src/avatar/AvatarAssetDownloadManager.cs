// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/AvatarAssetDownloadManager.as

using System.Linq;
using System.Xml.Linq;

using Vortex.Core.Assets;
using Vortex.Core.Assets.Loaders;
using Vortex.Core.Runtime.Events;
using Vortex.Habbo.Avatar.Events;
using Vortex.Habbo.Avatar.Structure;
using Vortex.Habbo.Avatar.Structure.Figure;

namespace Vortex.Habbo.Avatar;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/AvatarAssetDownloadManager.as
public class AvatarAssetDownloadManager
{
    public const string LIBRARY_LOADED = "LIBRARY_LOADED";

    private const int DOWNLOAD_TIMEOUT = 100;
    private const int MAX_SIMULTANEOUS_DOWNLOADS = 4;

    private readonly IAvatarRenderManager _renderManager;
    private readonly Dictionary<string, AvatarAssetDownloadLibrary> _libraries;
    private readonly Dictionary<string, List<AvatarAssetDownloadLibrary>> _partToLibraries;
    private readonly IAssetLibrary _assets;
    private readonly Dictionary<string, List<AvatarAssetDownloadLibrary>> _pendingFigures;
    private readonly Dictionary<string, List<IAvatarImageListener>> _listeners;
    private readonly AvatarStructure _structure;
    private readonly string _downloadUrl;
    private readonly string _urlTemplate;
    private bool _figureMapReady;
    private readonly List<string> _mandatoryLibraryNames;
    private readonly List<object[]> _initBuffer;
    private readonly List<AvatarAssetDownloadLibrary> _pendingQueue;
    private readonly List<AvatarAssetDownloadLibrary> _activeDownloads;
    private bool _disposed;

    public EventDispatcherWrapper Events { get; } = new();

    /// @see AvatarAssetDownloadManager.as::AvatarAssetDownloadManager
    public AvatarAssetDownloadManager
    (
        IAvatarRenderManager renderManager,
        IAssetLibrary assets,
        string figuremapUrl,
        string downloadUrl,
        AvatarStructure structure,
        string urlTemplate
    )
    {
        _mandatoryLibraryNames = ["hh_human_body", "hh_human_item"];
        _renderManager = renderManager;
        _libraries = new Dictionary<string, AvatarAssetDownloadLibrary>();
        _partToLibraries = new Dictionary<string, List<AvatarAssetDownloadLibrary>>();
        _assets = assets;
        _structure = structure;
        _downloadUrl = downloadUrl;
        _urlTemplate = urlTemplate;
        _pendingFigures = new Dictionary<string, List<AvatarAssetDownloadLibrary>>();
        _listeners = new Dictionary<string, List<IAvatarImageListener>>();
        _initBuffer = [];
        _pendingQueue = [];
        _activeDownloads = [];

        if (_renderManager is AvatarRenderManager concreteManager)
        {
            concreteManager.Events.AddEventListener(AvatarRenderEvent.AVATAR_RENDER_READY, PurgeInitDownloadBuffer);
        }

        // Try to load figuremap from existing assets
        IAsset? existingAsset = _assets.GetAssetByName("figuremap");

        if (existingAsset?.Content is XElement xml)
        {
            LoadFigureMapData(new XElement(xml));
        }
        else
        {
            // Attempt to load figuremap from file path
            try
            {
                BinaryFileLoader loader = new("text/xml", figuremapUrl);

                if (loader.Bytes is { Length: > 0 })
                {
                    string text = System.Text.Encoding.UTF8.GetString(loader.Bytes);
                    LoadFigureMapData(XElement.Parse(text));
                }
                else
                {
                    Logger.Warn("[AvatarAssetDownloadManager] figuremap not found at " + figuremapUrl);
                    _figureMapReady = true;
                    Events.DispatchEvent("complete");
                }

                loader.Dispose();
            }
            catch (System.Exception e)
            {
                Logger.Warn("[AvatarAssetDownloadManager] figuremap load failed: " + e.Message);
                _figureMapReady = true;
                Events.DispatchEvent("complete");
            }
        }
    }

    /// @see AvatarAssetDownloadManager.as::dispose
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Events.Dispose();
        _partToLibraries.Clear();
        _listeners.Clear();
        _pendingFigures.Clear();
        _pendingQueue.Clear();
        _initBuffer.Clear();
    }

    /// @see AvatarAssetDownloadManager.as::loadFigureMapData
    private void LoadFigureMapData(XElement? xml)
    {
        if (xml == null || string.IsNullOrEmpty(xml.ToString()))
        {
            return;
        }

        GenerateMap(xml);
        LoadMandatoryLibs();
        _figureMapReady = true;
        Events.DispatchEvent("complete");
    }

    /// @see AvatarAssetDownloadManager.as::loadMandatoryLibs
    public void LoadMandatoryLibs()
    {
        List<string> mandatoryCopy = new(_mandatoryLibraryNames);

        foreach (string name in mandatoryCopy)
        {
            if (_libraries.TryGetValue(name, out AvatarAssetDownloadLibrary? lib))
            {
                lib.IsMandatory = true;
                AddToQueue(lib);
            }
            else
            {
                Logger.Warn("[AvatarAssetDownloadManager] Missing mandatory library: " + name);
            }
        }

        ProcessPending();
    }

    /// @see AvatarAssetDownloadManager.as::generateMap
    private void GenerateMap(XElement xml)
    {
        foreach (XElement libXml in xml.Elements("lib"))
        {
            string id = (string?)libXml.Attribute("id") ?? "";
            string revision = (string?)libXml.Attribute("revision") ?? "";

            AvatarAssetDownloadLibrary lib = new(id, revision, _downloadUrl, _assets, _urlTemplate);
            lib.Events.AddEventListener("complete", _ => LibraryComplete(lib));
            _libraries[lib.LibraryName] = lib;

            foreach (XElement partXml in libXml.Elements("part"))
            {
                string key = (string?)partXml.Attribute("type") + ":" + (string?)partXml.Attribute("id");

                if (!_partToLibraries.TryGetValue(key, out List<AvatarAssetDownloadLibrary>? list))
                {
                    list = [];
                    _partToLibraries[key] = list;
                }

                list.Add(lib);
            }
        }
    }

    /// @see AvatarAssetDownloadManager.as::isReady
    public bool IsReady(IFigureContainer figure)
    {
        if (!_figureMapReady || !_structure.RenderManager.IsReady)
        {
            return false;
        }

        List<AvatarAssetDownloadLibrary> libs = GetLibsToDownload(figure);

        return libs.Count == 0;
    }

    /// @see AvatarAssetDownloadManager.as::loadFigureSetData
    public void LoadFigureSetData(IFigureContainer figure, IAvatarImageListener? listener)
    {
        if (!_figureMapReady || !_structure.RenderManager.IsReady)
        {
            _initBuffer.Add([figure, listener!]);
            return;
        }

        string figureString = figure.GetFigureString();
        List<AvatarAssetDownloadLibrary> libs = GetLibsToDownload(figure);

        if (libs.Count > 0)
        {
            if (listener is { disposed: false })
            {
                if (!_listeners.TryGetValue(figureString, out List<IAvatarImageListener>? listenerList))
                {
                    listenerList = [];
                    _listeners[figureString] = listenerList;
                }

                listenerList.Add(listener);
            }

            _pendingFigures[figureString] = libs;

            foreach (AvatarAssetDownloadLibrary lib in libs)
            {
                AddToQueue(lib);
            }

            ProcessPending();
        }
        else if (listener is
        {
            disposed: false,
        })
        {
            listener.AvatarImageReady(figureString);
        }
    }

    /// @see AvatarAssetDownloadManager.as::libraryComplete
    private void LibraryComplete(AvatarAssetDownloadLibrary completedLib)
    {
        if (_disposed)
        {
            return;
        }

        List<string> completedFigures = new();

        foreach ((string figureString, List<AvatarAssetDownloadLibrary> libs) in _pendingFigures)
        {
            bool allReady = libs.All(lib => lib.IsReady);

            if (!allReady)
            {
                continue;
            }

            completedFigures.Add(figureString);

            if (!_listeners.TryGetValue(figureString, out List<IAvatarImageListener>? listenerList))
            {
                continue;
            }

            foreach (IAvatarImageListener listener in listenerList.OfType<IAvatarImageListener>().Where(listener => !listener.disposed))
            {
                listener.AvatarImageReady(figureString);
            }

            _listeners.Remove(figureString);
        }

        foreach (string figureString in completedFigures)
        {
            _pendingFigures.Remove(figureString);
        }

        // Remove from mandatory list
        int mandatoryIndex = _mandatoryLibraryNames.IndexOf(completedLib.LibraryName);

        if (mandatoryIndex != -1)
        {
            _mandatoryLibraryNames.RemoveAt(mandatoryIndex);

            if (_mandatoryLibraryNames.Count == 0)
            {
                (_renderManager as AvatarRenderManager)?.OnMandatoryLibrariesReady();
            }
        }

        // Remove from active downloads
        for (int i = 0;
             i < _activeDownloads.Count;
             i++)
        {
            if (_activeDownloads[i].LibraryName != completedLib.LibraryName)
            {
                continue;
            }

            _activeDownloads.RemoveAt(i);

            break;
        }

        if (completedFigures.Count > 0)
        {
            Events.DispatchEvent(LIBRARY_LOADED, new LibraryLoadedEvent(LIBRARY_LOADED, completedLib.LibraryName));
        }

        ProcessPending();
    }

    /// @see AvatarAssetDownloadManager.as::isMissingMandatoryLibs
    public bool IsMissingMandatoryLibs()
    {
        return _mandatoryLibraryNames.Count > 0;
    }

    /// @see AvatarAssetDownloadManager.as::getLibsToDownload
    private List<AvatarAssetDownloadLibrary> GetLibsToDownload(IFigureContainer? figure)
    {
        List<AvatarAssetDownloadLibrary> result = new();

        if (_structure?.RenderManager == null || figure == null)
        {
            return result;
        }

        FigureSetData? figureData = _structure.FigureData;

        if (figureData == null)
        {
            return result;
        }

        string[] partTypeIds = figure.GetPartTypeIds();

        foreach (string typeId in partTypeIds)
        {
            ISetType? setType = figureData.GetSetType(typeId);

            if (setType == null)
            {
                continue;
            }

            int partSetId = figure.GetPartSetId(typeId);
            IFigurePartSet? partSet = setType.GetPartSet(partSetId);

            if (partSet == null)
            {
                continue;
            }

            foreach (IFigurePart part in partSet.Parts)
            {
                string key = part.Type + ":" + part.Id;

                if (!_partToLibraries.TryGetValue(key, out List<AvatarAssetDownloadLibrary>? libs))
                {
                    continue;
                }

                foreach (AvatarAssetDownloadLibrary lib in libs.OfType<AvatarAssetDownloadLibrary>()
                                                               .Where(lib => !lib.IsReady && !result.Contains(lib)))
                {
                    result.Add(lib);
                }
            }
        }

        return result;
    }

    /// @see AvatarAssetDownloadManager.as::processPending
    private void ProcessPending()
    {
        while (_pendingQueue.Count > 0 && _activeDownloads.Count < MAX_SIMULTANEOUS_DOWNLOADS)
        {
            AvatarAssetDownloadLibrary lib = _pendingQueue[0];

            _pendingQueue.RemoveAt(0);
            _activeDownloads.Add(lib);

            lib.StartDownloading();
        }
    }

    /// @see AvatarAssetDownloadManager.as::addToQueue
    private void AddToQueue(AvatarAssetDownloadLibrary lib)
    {
        if (!lib.IsReady && !_pendingQueue.Contains(lib) && !_activeDownloads.Contains(lib))
        {
            _pendingQueue.Add(lib);
        }
    }

    /// @see AvatarAssetDownloadManager.as::purgeInitDownloadBuffer
    private void PurgeInitDownloadBuffer(object? param)
    {
        foreach (object[] entry in _initBuffer)
        {
            LoadFigureSetData((IFigureContainer)entry[0], entry[1] as IAvatarImageListener);
        }

        _initBuffer.Clear();
    }

    /// @see AvatarAssetDownloadManager.as::purge
    public void Purge()
    {
        foreach (AvatarAssetDownloadLibrary lib in _libraries.Values.Where(lib => lib is { IsReady: true, IsMandatory: false }))
        {
            lib.Purge();
        }
    }
}
