using System;
using System.Globalization;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Assets.Loaders;
using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;
using Vortex.Room;
using Vortex.Room.Events;
using Vortex.Room.Object;
using Vortex.Room.Object.Visualization.Utils;
using Vortex.Habbo.Room.Object;
using Vortex.Habbo.Session;
using Vortex.Habbo.Session.Furniture;

using IRoomObjectVisualizationFactory = Vortex.Room.Object.IRoomObjectVisualizationFactory;

namespace Vortex.Habbo.Room;

/// @see com.sulake.habbo.room.RoomContentLoader
public class RoomContentLoader : IRoomContentLoader, IFurniDataListener
{
    public const string CONTENT_LOADER_READY = "RCL_LOADER_READY";

    private const string ASSET_LIBRARY_NAME_PREFIX = "RoomContentLoader ";

    private const int STATE_CREATED = 0;
    private const int STATE_INITIALIZING = 1;
    private const int STATE_READY = 2;

    private const string PLACE_HOLDER_FURNITURE = "place_holder";
    private const string PLACE_HOLDER_WALL_ITEM = "wall_place_holder";
    private const string PLACE_HOLDER_PET = "pet_place_holder";
    private const string PLACE_HOLDER_DEFAULT = "place_holder";
    private const string ROOM_CONTENT = "room";
    private const string TILE_CURSOR = "tile_cursor";
    private const string SELECTION_ARROW = "selection_arrow";

    private static readonly string[] PLACE_HOLDER_TYPES =
        ["place_holder", "wall_place_holder", "pet_place_holder", "room", "tile_cursor", "selection_arrow"];

    private const int CONTENT_DROP_DELAY = 20000;

    private readonly string _baseUrl;
    private Dictionary<string, IAssetLibrary>? _assetLibraries;
    private Dictionary<string, EventDispatcherWrapper>? _eventDispatchers;
    private Dictionary<string, string>? _extractedContentAliases;
    private Dictionary<string, IGraphicAssetCollection>? _graphicAssetCollections;
    private int _state;
    private EventDispatcherWrapper? _stateEvents;
    private bool _furniDataPopulated;

    private Dictionary<int, string>? _floorItemIdToType;
    private Dictionary<string, int>? _floorItems;
    private Dictionary<string, int>? _floorItemTypeToId;
    private Dictionary<int, string>? _wallItemIdToType;
    private Dictionary<string, int>? _wallItems;
    private Dictionary<string, int>? _wallItemTypeToId;
    private Dictionary<int, string>? _petTypeIndexToName;
    private Dictionary<string, int>? _petTypeNameToIndex;
    private Dictionary<int, Dictionary<string, PetColorResult>>? _petColors;
    private Dictionary<int, Dictionary<string, Dictionary<string, int>>>? _petLayers;
    private Dictionary<string, int>? _typeRevisions;
    private Dictionary<string, string>? _aliasForward;
    private Dictionary<string, string>? _aliasReverse;
    private Dictionary<string, string>? _adUrls;

    private string? _dynamicDownloadUrl;
    private string? _dynamicDownloadNameTemplate;
    private string? _dynamicIconDownloadNameTemplate;
    private string? _petDynamicDownloadUrl;
    private string? _petDynamicDownloadNameTemplate;
    private bool _deferredFurniData;
    private IAssetLibrary? _iconAssets;
    private IRoomContentListener? _iconListener;
    private ICoreConfiguration? _configuration;
    private string[]? _ignoredFurniTypes;
    private IRoomObjectVisualizationFactory? _visualizationFactory;
    private ISessionDataManager? _sessionDataManager;

    /// @see com.sulake.habbo.room.RoomContentLoader — set from RoomEngine after session data manager is ready
    public ISessionDataManager? SessionDataManager
    {
        get => _sessionDataManager;
        set
        {
            _sessionDataManager = value;

            if (_deferredFurniData)
            {
                _deferredFurniData = false;
                InitFurnitureData();
            }
        }
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::RoomContentLoader
    public RoomContentLoader(string baseUrl)
    {
        _baseUrl = baseUrl;
        _assetLibraries = new Dictionary<string, IAssetLibrary>(StringComparer.Ordinal);
        _eventDispatchers = new Dictionary<string, EventDispatcherWrapper>(StringComparer.Ordinal);
        _floorItemIdToType = new Dictionary<int, string>();
        _floorItemTypeToId = new Dictionary<string, int>(StringComparer.Ordinal);
        _floorItems = new Dictionary<string, int>(StringComparer.Ordinal);
        _wallItemIdToType = new Dictionary<int, string>();
        _wallItemTypeToId = new Dictionary<string, int>(StringComparer.Ordinal);
        _wallItems = new Dictionary<string, int>(StringComparer.Ordinal);
        _petTypeIndexToName = new Dictionary<int, string>();
        _petTypeNameToIndex = new Dictionary<string, int>(StringComparer.Ordinal);
        _adUrls = new Dictionary<string, string>(StringComparer.Ordinal);
        _typeRevisions = new Dictionary<string, int>(StringComparer.Ordinal);
        _aliasForward = new Dictionary<string, string>(StringComparer.Ordinal);
        _aliasReverse = new Dictionary<string, string>(StringComparer.Ordinal);
        _graphicAssetCollections = new Dictionary<string, IGraphicAssetCollection>(StringComparer.Ordinal);
        _extractedContentAliases = new Dictionary<string, string>(StringComparer.Ordinal);
    }

    public IRoomObjectVisualizationFactory? VisualizationFactory
    {
        set => _visualizationFactory = value;
    }

    public IAssetLibrary? IconAssets
    {
        set => _iconAssets = value;
    }

    public IRoomContentListener? IconListener
    {
        set => _iconListener = value;
    }

    public bool Disposed { get; private set; }

    /// @see com.sulake.habbo.room.RoomContentLoader::initialize
    public void Initialize(EventDispatcherWrapper events, ICoreConfiguration configuration)
    {
        _stateEvents = events;
        _dynamicDownloadUrl = configuration.GetProperty("flash.dynamic.download.url");
        _dynamicDownloadNameTemplate = configuration.GetProperty("flash.dynamic.download.name.template");
        _dynamicIconDownloadNameTemplate = configuration.GetProperty("flash.dynamic.icon.download.name.template");
        _petDynamicDownloadUrl = configuration.GetProperty("pet.dynamic.download.url");
        _petDynamicDownloadNameTemplate = configuration.GetProperty("pet.dynamic.download.name.template");
        _configuration = configuration;
        _state = STATE_INITIALIZING;
        InitFurnitureData();
        InitPetData(configuration);
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::dispose
    public void Dispose()
    {
        if (_assetLibraries != null)
        {
            foreach (IAssetLibrary lib in _assetLibraries.Values)
            {
                lib.Dispose();
            }

            _assetLibraries.Clear();
            _assetLibraries = null;
        }

        _eventDispatchers?.Clear();
        _eventDispatchers = null;

        _floorItemIdToType?.Clear();
        _floorItemIdToType = null;
        _floorItemTypeToId?.Clear();
        _floorItemTypeToId = null;
        _floorItems?.Clear();
        _floorItems = null;

        _wallItemIdToType?.Clear();
        _wallItemIdToType = null;
        _wallItemTypeToId?.Clear();
        _wallItemTypeToId = null;
        _wallItems?.Clear();
        _wallItems = null;

        _petTypeIndexToName?.Clear();
        _petTypeIndexToName = null;
        _petTypeNameToIndex?.Clear();
        _petTypeNameToIndex = null;

        _petColors?.Clear();
        _petColors = null;
        _petLayers?.Clear();
        _petLayers = null;

        _typeRevisions?.Clear();
        _typeRevisions = null;
        _aliasForward?.Clear();
        _aliasForward = null;
        _aliasReverse?.Clear();
        _aliasReverse = null;
        _adUrls?.Clear();
        _adUrls = null;

        if (_graphicAssetCollections != null)
        {
            foreach (IGraphicAssetCollection collection in _graphicAssetCollections.Values)
            {
                collection.Dispose();
            }

            _graphicAssetCollections.Clear();
            _graphicAssetCollections = null;
        }

        _extractedContentAliases?.Clear();
        _extractedContentAliases = null;

        _stateEvents = null;
        _configuration = null;
        Disposed = true;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::setRoomObjectAlias
    public void SetRoomObjectAlias(string original, string alias)
    {
        if (_aliasForward != null)
        {
            _aliasForward[original] = alias;
        }

        if (_aliasReverse != null)
        {
            _aliasReverse[alias] = original;
        }
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getObjectCategory
    public int GetObjectCategory(string? type)
    {
        if (type == null)
        {
            return -2;
        }

        if (_floorItems != null && _floorItems.ContainsKey(type))
        {
            return 10;
        }

        if (_wallItems != null && _wallItems.ContainsKey(type))
        {
            return 20;
        }

        if (_petTypeNameToIndex != null && _petTypeNameToIndex.ContainsKey(type))
        {
            return 100;
        }

        if (type.StartsWith("poster", StringComparison.Ordinal))
        {
            return 20;
        }

        return type switch
        {
            "room" => 0,
            "user" or "pet" or "bot" or "rentable_bot" => 100,
            "tile_cursor" or "selection_arrow" => 200,
            _ => -2,
        };
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getPlaceHolderType
    public string? GetPlaceHolderType(string type)
    {
        if (_floorItems != null && _floorItems.ContainsKey(type))
        {
            return PLACE_HOLDER_FURNITURE;
        }

        if (_wallItems != null && _wallItems.ContainsKey(type))
        {
            return PLACE_HOLDER_WALL_ITEM;
        }

        if (_petTypeNameToIndex != null && _petTypeNameToIndex.ContainsKey(type))
        {
            return PLACE_HOLDER_PET;
        }

        return PLACE_HOLDER_DEFAULT;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getPlaceHolderTypes
    public string[]? GetPlaceHolderTypes()
    {
        return PLACE_HOLDER_TYPES;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getActiveObjectType
    public string? GetActiveObjectType(int typeId)
    {
        if (_floorItemIdToType == null || !_floorItemIdToType.TryGetValue(typeId, out string? type))
        {
            Logger.Warn("[RoomContentLoader] Could not find type for id: " + typeId);
            return null;
        }

        return GetObjectType(type);
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getActiveObjectTypeId
    public int GetActiveObjectTypeId(string type)
    {
        if (_floorItemTypeToId != null && _floorItemTypeToId.TryGetValue(type, out int id))
        {
            return id;
        }

        return 0;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getWallItemType
    public string? GetWallItemType(int typeId, string? extra = null)
    {
        if (_wallItemIdToType == null || !_wallItemIdToType.TryGetValue(typeId, out string? type))
        {
            return null;
        }

        if (type == "poster" && extra != null)
        {
            type += extra;
        }

        return GetObjectType(type);
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getWallItemTypeId
    public int GetWallItemTypeId(string type)
    {
        if (_wallItemTypeToId != null && _wallItemTypeToId.TryGetValue(type, out int id))
        {
            return id;
        }

        return 0;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getPetType
    public string? GetPetType(int typeIndex)
    {
        if (_petTypeIndexToName != null && _petTypeIndexToName.TryGetValue(typeIndex, out string? name))
        {
            return name;
        }

        return null;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getPetTypeId
    public int GetPetTypeId(string type)
    {
        if (_petTypeNameToIndex != null && _petTypeNameToIndex.TryGetValue(type, out int index))
        {
            return index;
        }

        return 0;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getPetColor
    public PetColorResult? GetPetColor(int typeIndex, int paletteId)
    {
        if (_petColors != null && _petColors.TryGetValue(typeIndex, out Dictionary<string, PetColorResult>? palettes))
        {
            if (palettes.TryGetValue(paletteId.ToString(CultureInfo.InvariantCulture), out PetColorResult? result))
            {
                return result;
            }
        }

        return null;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getPetColorsByTag
    public List<PetColorResult> GetPetColorsByTag(int typeIndex, string tag)
    {
        List<PetColorResult> results = [];

        if (_petColors != null && _petColors.TryGetValue(typeIndex, out Dictionary<string, PetColorResult>? palettes))
        {
            foreach (PetColorResult color in palettes.Values)
            {
                if (color.Tag == tag)
                {
                    results.Add(color);
                }
            }
        }

        return results;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getPetLayerIdForTag
    public int GetPetLayerIdForTag(int typeIndex, string tag, int scale = 64)
    {
        if (_petLayers != null && _petLayers.TryGetValue(typeIndex, out Dictionary<string, Dictionary<string, int>>? layers))
        {
            string scaleKey = scale.ToString(CultureInfo.InvariantCulture);

            if (layers.TryGetValue(scaleKey, out Dictionary<string, int>? tagMap))
            {
                if (tagMap.TryGetValue(tag, out int layerId))
                {
                    return layerId;
                }
            }
        }

        return -1;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getPetDefaultPalette
    public PetColorResult? GetPetDefaultPalette(int typeIndex, string tag)
    {
        if (_petColors != null && _petColors.TryGetValue(typeIndex, out Dictionary<string, PetColorResult>? palettes))
        {
            foreach (PetColorResult color in palettes.Values)
            {
                if (Array.IndexOf(color.LayerTags, tag) > -1 && color.IsMaster)
                {
                    return color;
                }
            }
        }

        return null;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getActiveObjectColorIndex
    public int GetActiveObjectColorIndex(int typeId)
    {
        if (_floorItemIdToType != null && _floorItemIdToType.TryGetValue(typeId, out string? type))
        {
            return GetObjectColorIndex(type);
        }

        return -1;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getWallItemColorIndex
    public int GetWallItemColorIndex(int typeId)
    {
        if (_wallItemIdToType != null && _wallItemIdToType.TryGetValue(typeId, out string? type))
        {
            return GetObjectColorIndex(type);
        }

        return -1;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getRoomObjectAdURL
    public string GetRoomObjectAdURL(string type)
    {
        if (_adUrls != null && _adUrls.TryGetValue(type, out string? url))
        {
            return url;
        }

        return "";
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getContentType
    public string? GetContentType(string type)
    {
        return type;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::hasInternalContent
    public bool HasInternalContent(string type)
    {
        type = RoomObjectUserTypes.GetVisualizationType(type);
        return type is "user" or "game_snowball" or "game_snowsplash";
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::insertObjectContent
    public bool InsertObjectContent(int category, int objectId, IAssetLibrary library)
    {
        string? type = GetAssetLibraryType(library);

        if (type == null)
        {
            return false;
        }

        switch (category)
        {
            case 10:
                _floorItemIdToType![objectId] = type;
                _floorItemTypeToId![type] = objectId;
                break;
            case 20:
                _wallItemIdToType![objectId] = type;
                break;
            default:
                throw new InvalidOperationException("Registering content library for unsupported category " + category + "!");
        }

        AssetLibraryCollection? collection = AddAssetLibraryCollection(type, null) as AssetLibraryCollection;

        if (collection != null)
        {
            collection.AddAssetLibrary(library);

            if (InitializeGraphicAssetCollection(type, library))
            {
                switch (category)
                {
                    case 10:
                        _floorItems!.TryAdd(type, 1);
                        break;
                    case 20:
                        _wallItems!.TryAdd(type, 1);
                        break;
                }

                RoomContentLoadedEvent evt = new(RoomContentLoadedEvent.CONTENT_LOAD_SUCCESS, type);
                EventDispatcherWrapper? dispatcher = GetAssetLibraryEventDispatcher(type, true);
                dispatcher?.DispatchEvent(evt);

                return true;
            }
        }

        return false;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::loadObjectContent
    public bool LoadObjectContent(string type, object? eventDispatcher)
    {
        if (string.IsNullOrEmpty(type))
        {
            Logger.Warn("[RoomContentLoader] Can not load content, object type unknown!");
            return false;
        }

        string resolvedType = type;

        if (type.Contains(',', StringComparison.Ordinal))
        {
            resolvedType = type.Split(',')[0];
        }

        if (GetAssetLibrary(resolvedType) != null || GetAssetLibraryEventDispatcher(resolvedType) != null)
        {
            return false;
        }

        EventDispatcherWrapper? dispatcher = eventDispatcher as EventDispatcherWrapper;
        IAssetLibrary? collection = AddAssetLibraryCollection(resolvedType, dispatcher);

        if (collection == null)
        {
            return false;
        }

        if (IsIgnoredFurniType(resolvedType))
        {
            Logger.Warn("Ignored object type found from configuration. Not downloading assets for: " + resolvedType);
            return false;
        }

        // Get URLs for this content type
        string[] urls = GetObjectContentURLs(resolvedType);

        if (urls.Length == 0)
        {
            Logger.Warn($"[RoomContentLoader] No URLs for content type: {resolvedType}");
            return false;
        }

        // Download and load each URL
        foreach (string url in urls)
        {
            try
            {
                VortexBundleFileLoader loader = new(VortexBundleAsset.MIME_TYPE, url);

                if (loader.Content == null)
                {
                    Logger.Warn($"[RoomContentLoader] Bundle not found: {url}");
                    loader.Dispose();

                    RoomContentLoadedEvent failEvt = new(RoomContentLoadedEvent.CONTENT_LOAD_FAILURE, resolvedType);
                    GetAssetLibraryEventDispatcher(resolvedType, true)?.DispatchEvent(failEvt);
                    return false;
                }

                // Create asset library from bundle
                AssetLibrary lib = new(resolvedType);
                VortexBundleAsset bundleAsset = new(url: resolvedType);
                bundleAsset.SetUnknownContent(loader.Content);
                bundleAsset.PopulateLibrary(lib, resolvedType);

                // Add to collection
                if (collection is AssetLibraryCollection alc)
                {
                    alc.AddAssetLibrary(lib);
                }

                loader.Dispose();
            }
            catch (Exception e)
            {
                Logger.Warn($"[RoomContentLoader] Failed to load bundle {url}: {e.Message}");

                RoomContentLoadedEvent failEvt = new(RoomContentLoadedEvent.CONTENT_LOAD_FAILURE, resolvedType);
                GetAssetLibraryEventDispatcher(resolvedType, true)?.DispatchEvent(failEvt);
                return false;
            }
        }

        // Process the loaded library — initializes graphic asset collection and fires events
        ProcessLoadedLibrary(collection);
        return true;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::extractObjectContent
    public bool ExtractObjectContent(string source, string target)
    {
        IAssetLibrary? lib = GetAssetLibrary(source);
        _extractedContentAliases![target] = source;

        if (InitializeGraphicAssetCollection(target, lib))
        {
            return true;
        }

        _extractedContentAliases.Remove(target);
        return false;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getVisualizationType
    public string? GetVisualizationType(string type)
    {
        if (type == null)
        {
            return null;
        }

        IAssetLibrary? lib = GetAssetLibrary(type);

        if (lib == null)
        {
            return null;
        }

        IAsset? asset = lib.GetAssetByName(type + "_index") ?? lib.GetAssetByName("index");

        if (asset == null)
        {
            return null;
        }

        XElement? xml = asset.Content as XElement;
        return xml?.Attribute("visualization")?.Value;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getLogicType
    public string? GetLogicType(string type)
    {
        if (type == null)
        {
            return null;
        }

        IAssetLibrary? lib = GetAssetLibrary(type);

        if (lib == null)
        {
            return null;
        }

        IAsset? asset = lib.GetAssetByName(type + "_index") ?? lib.GetAssetByName("index");

        if (asset == null)
        {
            return null;
        }

        XElement? xml = asset.Content as XElement;
        return xml?.Attribute("logic")?.Value;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::hasVisualizationXML
    public bool HasVisualizationXml(string type)
    {
        return HasXml(type, "_visualization");
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getVisualizationXML
    public XElement? GetVisualizationXml(string type)
    {
        return GetXml(type, "_visualization");
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::hasAssetXML
    public bool HasAssetXml(string type)
    {
        return HasXml(type, "binaryData");
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getAssetXML
    public XElement? GetAssetXml(string type)
    {
        return GetXml(type, "binaryData");
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::hasLogicXML
    public bool HasLogicXml(string type)
    {
        return HasXml(type, "_logic");
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getLogicXML
    public XElement? GetLogicXml(string type)
    {
        return GetXml(type, "_logic");
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::addGraphicAsset
    public bool AddGraphicAsset(string type, string name, Image data, bool overwrite, bool recycle = true)
    {
        IGraphicAssetCollection? collection = GetGraphicAssetCollection(type);

        if (collection != null)
        {
            return collection.AddAsset(name, data, overwrite);
        }

        return false;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getGraphicAssetCollection
    public IGraphicAssetCollection? GetGraphicAssetCollection(string type)
    {
        string? contentType = GetContentType(type);

        if (contentType != null && _graphicAssetCollections != null &&
            _graphicAssetCollections.TryGetValue(contentType, out IGraphicAssetCollection? collection))
        {
            return collection;
        }

        return null;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::roomObjectCreated
    public void RoomObjectCreated(IRoomObject obj, string type)
    {
        IRoomObjectController? controller = obj as IRoomObjectController;

        if (controller?.ModelController != null)
        {
            controller.ModelController.SetString("object_room_id", type, true);
        }
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::setActiveObjectType
    public void SetActiveObjectType(int typeId, string type)
    {
        if (_floorItemIdToType != null)
        {
            _floorItemIdToType[typeId] = type;
        }
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::purge
    public void Purge()
    {
        if (_graphicAssetCollections == null || _assetLibraries == null)
        {
            return;
        }

        long now = System.Environment.TickCount64;
        List<string> toRemove = [];

        foreach (KeyValuePair<string, IGraphicAssetCollection> kvp in _graphicAssetCollections)
        {
            if (Array.IndexOf(PLACE_HOLDER_TYPES, kvp.Key) >= 0)
            {
                continue;
            }

            if (kvp.Value.ReferenceCount < 1 && now - kvp.Value.LastReferenceTimeStamp >= CONTENT_DROP_DELAY)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (string key in toRemove)
        {
            if (_graphicAssetCollections.Remove(key, out IGraphicAssetCollection? collection))
            {
                collection.Dispose();
            }

            string libName = GetAssetLibraryName(key);

            if (_assetLibraries.Remove(libName, out IAssetLibrary? lib))
            {
                lib.Dispose();
            }
        }
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::initPetData
    private void InitPetData(ICoreConfiguration configuration)
    {
        string? petConfig = configuration.GetProperty("pet.configuration");

        if (string.IsNullOrEmpty(petConfig))
        {
            return;
        }

        string[] petTypes = petConfig.Split(',');
        int index = 0;

        foreach (string petType in petTypes)
        {
            string trimmed = petType.Trim();
            _petTypeNameToIndex![trimmed] = index;
            _petTypeIndexToName![index] = trimmed;
            index++;
        }

        _petColors = new Dictionary<int, Dictionary<string, PetColorResult>>();
        _petLayers = new Dictionary<int, Dictionary<string, Dictionary<string, int>>>();
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::initFurnitureData
    private void InitFurnitureData()
    {
        if (SessionDataManager == null)
        {
            _deferredFurniData = true;
            return;
        }

        IFurnitureData[]? furniData = SessionDataManager.GetFurniData(this);

        if (furniData == null)
        {
            return;
        }

        SessionDataManager.RemoveFurniDataListener(this);
        PopulateFurniData(furniData);
        _furniDataPopulated = true;
        ParseIgnoredFurniTypes();
        ContinueInitialization();
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::furniDataReady
    public void FurniDataReceived()
    {
        InitFurnitureData();
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::populateFurniData
    private void PopulateFurniData(IEnumerable<IFurnitureData> furniData)
    {
        foreach (IFurnitureData data in furniData)
        {
            int id = data.id;
            string className = data.className;

            if (data.hasIndexedColor)
            {
                className += "*" + data.colourIndex.ToString(CultureInfo.InvariantCulture);
            }

            int revision = data.revision;
            string adUrl = data.adUrl;

            if (!string.IsNullOrEmpty(adUrl))
            {
                _adUrls![className] = adUrl;
            }

            string revisionKey = data.className;

            if (data.type == "s")
            {
                _floorItemIdToType![id] = className;
                _floorItemTypeToId![className] = id;

                if (!_floorItems!.ContainsKey(revisionKey))
                {
                    _floorItems[revisionKey] = 1;
                }
            }
            else if (data.type == "i")
            {
                if (className == "post.it")
                {
                    className = "post_it";
                    revisionKey = "post_it";
                }

                if (className == "post.it.vd")
                {
                    className = "post_it_vd";
                    revisionKey = "post_it_vd";
                }

                _wallItemIdToType![id] = className;
                _wallItemTypeToId![className] = id;

                if (!_wallItems!.ContainsKey(revisionKey))
                {
                    _wallItems[revisionKey] = 1;
                }
            }

            _typeRevisions!.TryGetValue(revisionKey, out int previousRevision);

            if (revision > previousRevision)
            {
                _typeRevisions[revisionKey] = revision;
            }
        }
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::parseIgnoredFurniTypes
    private void ParseIgnoredFurniTypes()
    {
        string? ignoredConfig = _configuration?.GetProperty("gpu.ignored_furni");

        if (string.IsNullOrEmpty(ignoredConfig))
        {
            return;
        }

        string[] types = ignoredConfig.Split(',');

        for (int i = 0; i < types.Length; i++)
        {
            types[i] = types[i].Trim();
        }

        _ignoredFurniTypes = types;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::isIgnoredFurniType
    private bool IsIgnoredFurniType(string type)
    {
        return _ignoredFurniTypes != null && Array.IndexOf(_ignoredFurniTypes, type) != -1;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::continueInitilization
    private void ContinueInitialization()
    {
        if (_furniDataPopulated)
        {
            _state = STATE_READY;
            _stateEvents?.DispatchEvent(CONTENT_LOADER_READY);
        }
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getRoomObjectAlias
    private string GetRoomObjectAlias(string type)
    {
        if (_aliasForward != null && _aliasForward.TryGetValue(type, out string? alias))
        {
            return alias;
        }

        return type;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getRoomObjectOriginalName
    private string GetRoomObjectOriginalName(string type)
    {
        if (_aliasReverse != null && _aliasReverse.TryGetValue(type, out string? original))
        {
            return original;
        }

        return type;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getObjectType
    private static string? GetObjectType(string? type)
    {
        if (type == null)
        {
            return null;
        }

        int starIndex = type.IndexOf('*', StringComparison.Ordinal);

        if (starIndex >= 0)
        {
            type = type[..starIndex];
        }

        return type;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getObjectColorIndex
    private static int GetObjectColorIndex(string? type)
    {
        if (type == null)
        {
            return -1;
        }

        int starIndex = type.IndexOf('*', StringComparison.Ordinal);

        if (starIndex >= 0 && int.TryParse(type.AsSpan(starIndex + 1), out int colorIndex))
        {
            return colorIndex;
        }

        return 0;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getObjectRevision
    private int GetObjectRevision(string type)
    {
        int category = GetObjectCategory(type);

        if (category is 10 or 20)
        {
            if (type.StartsWith("poster", StringComparison.Ordinal))
            {
                type = "poster";
            }

            if (_typeRevisions != null && _typeRevisions.TryGetValue(type, out int revision))
            {
                return revision;
            }
        }

        return 0;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getObjectContentURLs
    private string[] GetObjectContentURLs(string type, string? extra = null, bool isIcon = false)
    {
        string? contentType = GetContentType(type);

        if (contentType == null)
        {
            return [];
        }

        switch (contentType)
        {
            case PLACE_HOLDER_FURNITURE:
                return [ResolveContentUrl("PlaceHolderFurniture.vortex")];
            case PLACE_HOLDER_WALL_ITEM:
                return [ResolveContentUrl("PlaceHolderWallItem.vortex")];
            case PLACE_HOLDER_PET:
                return [ResolveContentUrl("PlaceHolderPet.vortex")];
            case ROOM_CONTENT:
                return [ResolveContentUrl("HabboRoomContent.vortex")];
            case TILE_CURSOR:
                return [ResolveContentUrl("TileCursor.vortex")];
            case SELECTION_ARROW:
                return [ResolveContentUrl("SelectionArrow.vortex")];
            default:
                {
                    int category = GetObjectCategory(contentType);

                    switch (category)
                    {
                        case 10 or 20:
                            {
                                string alias = GetRoomObjectAlias(contentType);
                                string? template = isIcon ? _dynamicIconDownloadNameTemplate : _dynamicDownloadNameTemplate;

                                if (template == null)
                                {
                                    return [];
                                }

                                template = template.Replace("%typeid%", alias, StringComparison.Ordinal);
                                int revision = GetObjectRevision(contentType);
                                template = template.Replace("%revision%", revision.ToString(CultureInfo.InvariantCulture),
                                    StringComparison.Ordinal);

                                if (!isIcon)
                                {
                                    return [(_dynamicDownloadUrl ?? "") + template];
                                }
                                bool hasColor = extra is
                                                {
                                                    Length: > 0,
                                                } &&
                                                _floorItemTypeToId != null &&
                                                _floorItemTypeToId.ContainsKey(type + "*" + extra);
                                template = template.Replace("%param%",
                                    hasColor ? "_" + extra : "", StringComparison.Ordinal);

                                return [(_dynamicDownloadUrl ?? "") + template];
                            }
                        case 100:
                            {
                                string url = (_petDynamicDownloadUrl ?? "") + (_petDynamicDownloadNameTemplate ?? "");
                                url = url.Replace("%type%", contentType, StringComparison.Ordinal);

                                return [url];
                            }
                        default:
                            return [];
                    }

                }
        }
    }

    /// <summary>
    /// Resolves a bundle filename to a local <c>res://</c> path if available,
    /// otherwise prepends the dynamic download URL.
    /// </summary>
    private string ResolveContentUrl(string filename)
    {
        string localPath = "res://data/bundles/" + filename;

        if (Godot.FileAccess.FileExists(localPath))
        {
            return localPath;
        }

        return (_dynamicDownloadUrl ?? "") + filename;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::extractPetDataFromLoadedContent
    private void ExtractPetDataFromLoadedContent(string type)
    {
        if (_petTypeNameToIndex == null || !_petTypeNameToIndex.TryGetValue(type, out int typeIndex))
        {
            return;
        }

        IGraphicAssetCollection? collection = GetGraphicAssetCollection(type);

        if (collection != null)
        {
            Dictionary<string, PetColorResult> colorMap = new(StringComparer.Ordinal);
            string[]? paletteNames = collection.GetPaletteNames();

            if (paletteNames != null)
            {
                foreach (string paletteName in paletteNames)
                {
                    int[]? colors = collection.GetPaletteColors(paletteName);

                    if (colors != null && colors.Length >= 2)
                    {
                        XElement? paletteXml = collection.GetPaletteXml(paletteName);

                        if (paletteXml != null)
                        {
                            int breed = int.TryParse(paletteXml.Attribute("breed")?.Value, out int b) ? b : 0;
                            int colorTag = int.TryParse(paletteXml.Attribute("colortag")?.Value, out int ct) ? ct : -1;
                            string[] tags = paletteXml.Attribute("tags")?.Value?.Split(',') ?? [];
                            bool isMaster = paletteXml.Attribute("master")?.Value == "true";

                            colorMap[paletteName] = new PetColorResult(colors[0], colors[1], breed, colorTag, paletteName, isMaster, tags);
                        }
                    }
                }
            }

            _petColors![typeIndex] = colorMap;
        }

        XElement? visXml = GetVisualizationXml(type);

        if (visXml != null)
        {
            Dictionary<string, Dictionary<string, int>> layerMap = new(StringComparer.Ordinal);

            foreach (XElement visualization in visXml.Elements("visualization"))
            {
                Dictionary<string, int> tagMap = new(StringComparer.Ordinal);

                XElement? layersEl = visualization.Element("layers");

                if (layersEl != null)
                {
                    foreach (XElement layer in layersEl.Elements("layer"))
                    {
                        string? tag = layer.Attribute("tag")?.Value;

                        if (tag != null && int.TryParse(layer.Attribute("id")?.Value, out int layerId))
                        {
                            tagMap[tag] = layerId;
                        }
                    }
                }

                string? size = visualization.Attribute("size")?.Value;

                if (size != null)
                {
                    layerMap[size] = tagMap;
                }
            }

            _petLayers![typeIndex] = layerMap;
        }
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::initializeGraphicAssetCollection
    private bool InitializeGraphicAssetCollection(string type, IAssetLibrary? library)
    {
        if (type == null || library == null)
        {
            return false;
        }

        IGraphicAssetCollection? collection = CreateGraphicAssetCollection(type, library);

        if (collection != null)
        {
            XElement? assetXml = GetAssetXml(type);

            if (assetXml != null && collection.Define(assetXml))
            {
                return true;
            }

            DisposeGraphicAssetCollection(type);
        }

        return false;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getAssetLibraryName
    private static string GetAssetLibraryName(string type)
    {
        return ASSET_LIBRARY_NAME_PREFIX + type;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getAssetLibrary
    private IAssetLibrary? GetAssetLibrary(string type)
    {
        string? contentType = GetContentType(type);

        if (contentType == null)
        {
            return null;
        }

        contentType = GetRoomObjectOriginalName(contentType);
        string libName = GetAssetLibraryName(contentType);

        if (_assetLibraries != null && _assetLibraries.TryGetValue(libName, out IAssetLibrary? lib))
        {
            return lib;
        }

        if (_extractedContentAliases != null && _extractedContentAliases.TryGetValue(contentType, out string? alias))
        {
            string? aliasType = GetContentType(alias);

            if (aliasType != null)
            {
                string aliasLibName = GetAssetLibraryName(aliasType);

                if (_assetLibraries != null && _assetLibraries.TryGetValue(aliasLibName, out lib))
                {
                    return lib;
                }
            }
        }

        return null;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::addAssetLibraryCollection
    private IAssetLibrary? AddAssetLibraryCollection(string type, EventDispatcherWrapper? eventDispatcher)
    {
        string? contentType = GetContentType(type);

        if (contentType == null)
        {
            return null;
        }

        IAssetLibrary? existing = GetAssetLibrary(type);

        if (existing != null)
        {
            return existing;
        }

        string libName = GetAssetLibraryName(contentType);
        AssetLibraryCollection collection = new(libName);
        _assetLibraries![libName] = collection;

        if (eventDispatcher != null && GetAssetLibraryEventDispatcher(type) == null)
        {
            _eventDispatchers![contentType] = eventDispatcher;
        }

        return collection;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getAssetLibraryEventDispatcher
    private EventDispatcherWrapper? GetAssetLibraryEventDispatcher(string type, bool remove = false)
    {
        string? contentType = GetContentType(type);

        if (contentType == null || _eventDispatchers == null)
        {
            return null;
        }

        if (!remove)
        {
            return _eventDispatchers.GetValueOrDefault(contentType);
        }

        _eventDispatchers.Remove(contentType, out EventDispatcherWrapper? dispatcher);
        return dispatcher;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getAssetLibraryType
    private static string? GetAssetLibraryType(IAssetLibrary? library)
    {
        if (library == null)
        {
            return null;
        }

        IAsset? indexAsset = library.GetAssetByName("index");

        if (indexAsset == null)
        {
            return null;
        }

        XElement? xml = indexAsset.Content as XElement;
        return xml?.Attribute("type")?.Value;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::getXML
    private XElement? GetXml(string type, string suffix)
    {
        IAssetLibrary? lib = GetAssetLibrary(type);

        if (lib == null)
        {
            return null;
        }

        string? contentType = GetContentType(type);

        if (contentType == null)
        {
            return null;
        }

        string alias = GetRoomObjectAlias(contentType);
        IAsset? asset = lib.GetAssetByName(alias + suffix);

        if (asset == null)
        {
            return null;
        }

        return asset.Content as XElement;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::hasXML
    private bool HasXml(string type, string suffix)
    {
        IAssetLibrary? lib = GetAssetLibrary(type);

        if (lib == null)
        {
            return false;
        }

        string? contentType = GetContentType(type);

        if (contentType == null)
        {
            return false;
        }

        string alias = GetRoomObjectAlias(contentType);
        return lib.HasAsset(alias + suffix);
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::createGraphicAssetCollection
    private IGraphicAssetCollection? CreateGraphicAssetCollection(string type, IAssetLibrary? library)
    {
        IGraphicAssetCollection? existing = GetGraphicAssetCollection(type);

        if (existing != null)
        {
            return existing;
        }

        if (library == null || _visualizationFactory == null)
        {
            return null;
        }

        IGraphicAssetCollection? collection = _visualizationFactory.CreateGraphicAssetCollection();

        if (collection != null)
        {
            collection.AssetLibrary = library;
            _graphicAssetCollections![type] = collection;
        }

        return collection;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::disposeGraphicAssetCollection
    private bool DisposeGraphicAssetCollection(string type)
    {
        string? contentType = GetContentType(type);

        if (contentType != null && _graphicAssetCollections != null &&
            _graphicAssetCollections.Remove(contentType, out IGraphicAssetCollection? collection))
        {
            collection.Dispose();
            return true;
        }

        return false;
    }

    /// @see com.sulake.habbo.room.RoomContentLoader::processLoadedLibrary
    private void ProcessLoadedLibrary(IAssetLibrary library)
    {
        string? type = GetAssetLibraryType(library);

        if (type == null)
        {
            return;
        }

        type = GetRoomObjectOriginalName(type);
        bool success = InitializeGraphicAssetCollection(type, library);
        RoomContentLoadedEvent evt;

        if (success)
        {
            if (_petTypeNameToIndex != null && _petTypeNameToIndex.ContainsKey(type))
            {
                ExtractPetDataFromLoadedContent(type);
            }

            evt = new RoomContentLoadedEvent(RoomContentLoadedEvent.CONTENT_LOAD_SUCCESS, type);
        }
        else
        {
            evt = new RoomContentLoadedEvent(RoomContentLoadedEvent.CONTENT_LOAD_FAILURE, type);
        }

        EventDispatcherWrapper? dispatcher = GetAssetLibraryEventDispatcher(type, true);
        dispatcher?.DispatchEvent(evt);
    }
}
