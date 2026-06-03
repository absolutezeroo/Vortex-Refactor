// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/class_1808.as

using System;
using System.Linq;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Assets.Loaders;
using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;
using Vortex.Habbo.Avatar.Alias;
using Vortex.Habbo.Avatar.Animation;
using Vortex.Habbo.Avatar.Events;
using Vortex.Habbo.Avatar.Structure;
using Vortex.Habbo.Avatar.Structure.Figure;
using Vortex.IID;

namespace Vortex.Habbo.Avatar;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/class_1808.as
public class AvatarRenderManager : Component, IAvatarRenderManager
{
    private const string AVATAR_PLACEHOLDER_FIGURE = "hd-99999-99999";

    private AssetAliasCollection? _aliasCollection;
    private AvatarStructure? _structure;
    private AvatarAssetDownloadManager? _avatarAssetDownloadManager;
    private EffectAssetDownloadManager? _effectAssetDownloadManager;
    private AvatarFigureContainer? _placeholderFigure;
    private bool _avatarAssetsReady;
    private bool _figureDataReady;
    private bool _actionsReady;
    private bool _effectAssetsReady;
    private readonly bool _inNuxFlow;
    private readonly List<object[]> _initBuffer;
    private Dictionary<int, object>? _effectMapCache;

    /// @see class_1808.as::class_1808
    public AvatarRenderManager(IContext param1, uint param2 = 0, object? param3 = null, bool param4 = false)
        : base(param1, param2 | COMPONENT_FLAG_DISPOSABLE, param3)
    {
        _initBuffer =
        [
        ];
        _inNuxFlow = param4;
    }

    /// @see class_1808.as::get dependencies
    protected override IList<ComponentDependency> dependencies
    {
        get
        {
            if (_inNuxFlow)
            {
                return base.dependencies;
            }

            List<ComponentDependency> baseDeps = new(base.dependencies)
            {
                new(
                    new IIDHabboConfigurationManager(),
                    null,
                    true,
                    [
                        new DependencyEventListener("complete", OnConfigurationComplete),
                    ]
                ),
            };

            return baseDeps;
        }
    }

    /// @see class_1808.as::get isReady
    public bool IsReady { get; private set; }

    /// @see class_1808.as::initComponent
    protected override void InitComponent()
    {
        Mode = "component";

        // AS3: initComponent uses baked-in actions (Default + 5 SnowWar).
        // Full actions are downloaded later via onConfigurationComplete() → requestActions().
        XElement bakedInActionsXml = XElement.Parse(
            """
            <actions>
                <action id="Default" precedence="1000" state="std" main="1" isdefault="1" geometrytype="vertical" activepartset="figure" assetpartdefinition="std"/>
                <action id="SnowWarRun" state="swrun" precedence="104" main="1" geometrytype="vertical" activepartset="snowwarrun" assetpartdefinition="swrun"/>
                <action id="SnowWarDieFront" state="swdiefront" precedence="105" main="1" geometrytype="swhorizontal" activepartset="snowwardiefront" assetpartdefinition="swdie" startfromframezero="true"/>
                <action id="SnowWarDieBack" state="swdieback" precedence="106" main="1" geometrytype="swhorizontal" activepartset="snowwardieback" assetpartdefinition="swdie" startfromframezero="true"/>
                <action id="SnowWarPick" state="swpick" precedence="107" main="1" geometrytype="vertical" activepartset="snowwarpick" assetpartdefinition="swpick" startfromframezero="true"/>
                <action id="SnowWarThrow" state="swthrow" precedence="108" main="1" geometrytype="vertical" activepartset="snowwarthrow" assetpartdefinition="swthrow" startfromframezero="true"/>
            </actions>
            """
        );

        _structure = new AvatarStructure(this);

        IAssetLibrary? assetLib = assets as IAssetLibrary;

        _structure.InitGeometry(GetAssetXml(assetLib, "HabboAvatarGeometry"));
        _structure.InitPartSets(GetAssetXml(assetLib, "HabboAvatarPartSets"));
        _structure.InitActions(assetLib!, bakedInActionsXml);
        _structure.InitAnimation(GetAssetXml(assetLib, "HabboAvatarAnimation"));
        _structure.InitFigureData(GetAssetXml(assetLib, "HabboAvatarFigure"));

        // AS3: no requestActions() here — that's called in onConfigurationComplete()

        _aliasCollection = new AssetAliasCollection(
            this, context.assets as AssetLibraryCollection ?? new AssetLibraryCollection("empty")
        );
        _aliasCollection.Init();

        CheckIfReady();
    }

    /// @see class_1808.as::_Str_1200 (requestActions)
    /// AS3: assets.loadAssetFromFile("HabboAvatarActions", new URLRequest(url), "text/xml")
    private void RequestActions()
    {
        string? downloadUrl = GetProperty("flash.dynamic.avatar.download.url");
        string url = (downloadUrl ?? "") + "HabboAvatarActions.xml";

        if (assets is IAssetLibrary assetLib)
        {
            AssetLoaderStruct? loaderStruct = assetLib.LoadAssetFromFile("HabboAvatarActions", url, "text/xml");

            if (loaderStruct != null)
            {
                loaderStruct.LoaderEvent += OnActionsLoadEvent;
                return;
            }
        }

        // Loader failed to create (no URL, no loader type, etc.) — fall back to local file
        Logger.Warn("[AvatarRenderManager] LoadAssetFromFile returned null, falling back to local actions XML");
        OnAvatarActionsLoaded();
    }

    /// Handler for the async load event from RequestActions.
    private void OnActionsLoadEvent(AssetLoaderEvent evt)
    {
        if (evt.Type == AssetLoaderEvent.ASSET_LOADER_EVENT_COMPLETE)
        {
            OnAvatarActionsLoaded();
        }
        else
        {
            // Download failed — fall back to local file
            Logger.Warn("[AvatarRenderManager] Actions download failed, falling back to local actions XML");
            OnAvatarActionsLoaded();
        }
    }

    /// @see class_1808.as::onAvatarAssetsLibraryReady (misnamed in AS3 — this handles actions, not avatar assets)
    private void OnAvatarActionsLoaded()
    {
        if (_structure == null)
        {
            return;
        }

        // AS3: assets.getAssetByName("HabboAvatarActions").content as XML ?? minimalFallback
        XElement actionsXml = assets is IAssetLibrary assetLib && assetLib.HasAsset("HabboAvatarActions")
            ? GetAssetXml(assetLib, "HabboAvatarActions") ?? LoadActionsXml()
            : LoadActionsXml();

        _structure.UpdateActions(actionsXml);
        _actionsReady = true;
        CheckIfReady();
    }

    /// @see class_1808.as::dispose
    public override void Dispose()
    {
        base.Dispose();

        if (_structure != null)
        {
            _structure.Dispose();
            _structure = null;
        }

        if (_aliasCollection != null)
        {
            _aliasCollection.Dispose();
            _aliasCollection = null;
        }

        if (_avatarAssetDownloadManager != null)
        {
            _avatarAssetDownloadManager.Events.RemoveEventListener("complete", OnAvatarAssetsDownloadManagerReady);
            _avatarAssetDownloadManager.Dispose();
            _avatarAssetDownloadManager = null;
        }

        if (_effectAssetDownloadManager != null)
        {
            _effectAssetDownloadManager.Events.RemoveEventListener("complete", OnEffectAssetsDownloadManagerReady);
            _effectAssetDownloadManager.Dispose();
            _effectAssetDownloadManager = null;
        }
    }

    /// @see class_1808.as::onConfigurationComplete
    private void OnConfigurationComplete(object? param)
    {
        RequestActions();

        if (_structure == null)
        {
            return;
        }

        IAssetLibrary? assetLib = assets as IAssetLibrary;

        // Download external figure data
        string figurePartListUrl = GetProperty("external.figurepartlist.txt");

        if (assetLib != null && assetLib.HasAsset(figurePartListUrl))
        {
            assetLib.RemoveAsset(assetLib.GetAssetByName(figurePartListUrl)!);
        }

        AvatarStructureDownload structureDownload = new(
            assetLib!, figurePartListUrl, _structure.FigureData as IStructureData ?? _structure.FigureData!
        );
        structureDownload.Events.AddEventListener(AvatarStructureDownload.STRUCTURE_DONE, OnFigureDataDownloadDone);

        // Godot adaptation: synchronous downloads may complete during construction,
        // before listeners are attached. Check and manually fire handlers if needed.
        // (AS3 URLLoader is async, so listeners were always attached in time.)
        if (structureDownload.IsDone)
        {
            OnFigureDataDownloadDone(null);
        }

        // Create avatar asset download manager
        if (_avatarAssetDownloadManager == null)
        {
            string figuremapUrl = GetProperty("flash.dynamic.avatar.download.configuration");
            string downloadUrl = GetProperty("flash.dynamic.avatar.download.url");
            string urlTemplate = GetProperty("flash.dynamic.avatar.download.name.template");

            _avatarAssetDownloadManager = new AvatarAssetDownloadManager(
                this, context.assets as IAssetLibrary ?? assetLib!, figuremapUrl, downloadUrl, _structure, urlTemplate
            );
            _avatarAssetDownloadManager.Events.AddEventListener("complete", OnAvatarAssetsDownloadManagerReady);
            _avatarAssetDownloadManager.Events.AddEventListener(AvatarAssetDownloadManager.LIBRARY_LOADED, OnAvatarAssetsLibraryReady);

            // Synchronous completion: mandatory libs may already be downloaded
            if (!_avatarAssetDownloadManager.IsMissingMandatoryLibs())
            {
                OnAvatarAssetsDownloadManagerReady(null);
            }
        }

        // Create effect asset download manager
        if (_effectAssetDownloadManager == null)
        {
            string effectMapUrl = GetProperty("flash.dynamic.avatar.download.url") + "effectmap.xml";
            string downloadUrl = GetProperty("flash.dynamic.avatar.download.url");
            string urlTemplate = GetProperty("flash.dynamic.avatar.download.name.template");

            _effectAssetDownloadManager = new EffectAssetDownloadManager(
                context.assets as IAssetLibrary ?? assetLib!, effectMapUrl, downloadUrl, _structure, urlTemplate
            );
            _effectAssetDownloadManager.Events.AddEventListener("complete", OnEffectAssetsDownloadManagerReady);
            _effectAssetDownloadManager.Events.AddEventListener(EffectAssetDownloadManager.LIBRARY_LOADED, OnEffectAssetsLibraryReady);

            // Synchronous completion: effect map may already be loaded
            if (_effectAssetDownloadManager.IsMapReady)
            {
                OnEffectAssetsDownloadManagerReady(null);
            }
        }
    }

    /// @see class_1808.as::onMandatoryLibrariesReady
    public void OnMandatoryLibrariesReady()
    {
        CheckIfReady();
    }

    /// @see class_1808.as::onAvatarAssetsLibraryReady
    private void OnAvatarAssetsLibraryReady(object? param)
    {
        if (param is LibraryLoadedEvent evt)
        {
            _aliasCollection?.OnAvatarAssetsLibraryReady(evt.Library);
        }
    }

    /// @see class_1808.as::onEffectAssetsLibraryReady
    private void OnEffectAssetsLibraryReady(object? param)
    {
        if (param is LibraryLoadedEvent evt)
        {
            _aliasCollection?.OnAvatarAssetsLibraryReady(evt.Library);
        }
    }

    /// @see class_1808.as::onFigureDataDownloadDone
    private void OnFigureDataDownloadDone(object? param)
    {
        string url = GetProperty("external.figurepartlist.txt");

        if (assets is IAssetLibrary assetLib)
        {
            IAsset? asset = assetLib.GetAssetByName(url);

            if (asset != null)
            {
                assetLib.RemoveAsset(asset)?.Dispose();
            }
        }

        _figureDataReady = true;

        _structure?.Init();

        CheckIfReady();
    }

    /// @see class_1808.as::onAvatarAssetsDownloadManagerReady
    private void OnAvatarAssetsDownloadManagerReady(object? param)
    {
        _avatarAssetsReady = true;

        CheckIfReady();
    }

    /// @see class_1808.as::onEffectAssetsDownloadManagerReady
    private void OnEffectAssetsDownloadManagerReady(object? param)
    {
        _effectAssetsReady = true;

        CheckIfReady();
    }

    /// @see class_1808.as::checkIfReady
    private void CheckIfReady()
    {
        if (IsReady)
        {
            return;
        }

        if (!_avatarAssetsReady || !_figureDataReady || !_actionsReady || !_effectAssetsReady)
        {
            return;
        }

        IsReady = true;

        events.DispatchEvent(AvatarRenderEvent.AVATAR_RENDER_READY);

        PurgeInitDownloadBuffer();
    }

    /// @see class_1808.as::purgeInitDownloadBuffer
    private void PurgeInitDownloadBuffer()
    {
        foreach (object[] entry in _initBuffer)
        {
            if (entry[1] is IAvatarImageListener { disposed: false } listener)
            {
                _avatarAssetDownloadManager?.LoadFigureSetData(entry[0] as IFigureContainer ?? new AvatarFigureContainer(""), listener);
            }
        }

        _initBuffer.Clear();
    }

    /// @see class_1808.as::get effectMap
    public IDictionary<int, object>? EffectMap
    {
        get
        {
            if (!_effectAssetsReady || _effectAssetDownloadManager == null)
            {
                return null;
            }

            if (_effectMapCache != null)
            {
                return _effectMapCache;
            }

            // Adapt Dictionary<string, List<EffectAssetDownloadLibrary>> to IDictionary<int, object>
            _effectMapCache = new Dictionary<int, object>();

            foreach ((string key, List<EffectAssetDownloadLibrary> value) in _effectAssetDownloadManager.EffectMap)
            {
                if (int.TryParse(key, out int intKey))
                {
                    _effectMapCache[intKey] = value;
                }
            }

            return _effectMapCache;
        }
    }

    // --- IAvatarRenderManager implementation ---

    /// @see class_1808.as::createFigureContainer
    public IFigureContainer CreateFigureContainer(string param1)
    {
        return new AvatarFigureContainer(param1);
    }

    /// @see class_1808.as::isFigureReady
    public bool IsFigureReady(IFigureContainer param1)
    {
        return _avatarAssetDownloadManager != null && _avatarAssetDownloadManager.IsReady(param1);
    }

    /// @see class_1808.as::downloadFigure
    public void DownloadFigure(IFigureContainer param1, IAvatarImageListener param2)
    {
        if (_avatarAssetDownloadManager == null)
        {
            _initBuffer.Add(
                [
                    param1, param2,
                ]
            );

            return;
        }

        _avatarAssetDownloadManager.LoadFigureSetData(param1, param2);
    }

    /// @see class_1808.as::createAvatarImage
    public IAvatarImage? CreateAvatarImage
    (
        string param1,
        string param2,
        string? param3 = null,
        IAvatarImageListener? param4 = null,
        IAvatarEffectListener? param5 = null
    )
    {
        AvatarFigureContainer figure = new(param1);

        // AS3: if(param3) { validateAvatarFigure(_loc7_, param3); }
        if (!string.IsNullOrEmpty(param3))
        {
            ValidateAvatarFigure(figure, param3);
        }

        if (_structure == null || (_avatarAssetDownloadManager == null && Mode != "local_only"))
        {
            _initBuffer.Add(
                [
                    figure, param4!,
                ]
            );

            return null;
        }

        if (Mode == "local_only" || (_avatarAssetDownloadManager?.IsReady(figure) ?? false))
        {
            return new AvatarImage(_structure, _aliasCollection!, figure, param2, _effectAssetDownloadManager!, param5);
        }

        // Not ready yet — queue download and return placeholder
        // AS3 uses a fixed placeholder figure "hd-99999-99999", not the actual requested figure
        _placeholderFigure ??= new AvatarFigureContainer("hd-99999-99999");
        _avatarAssetDownloadManager?.LoadFigureSetData(figure, param4);
        return new PlaceholderAvatarImage(_structure, _aliasCollection!, _placeholderFigure, param2, _effectAssetDownloadManager!);
    }

    /// @see class_1808.as::getFigureData
    public IFigureData? GetFigureData()
    {
        return _structure?.FigureData;
    }

    /// @see class_1808.as::isValidFigureSetForGender
    public bool IsValidFigureSetForGender(int param1, string param2)
    {
        FigureSetData? figureData = GetFigureData() as FigureSetData;
        IFigurePartSet? partSet = figureData?.GetFigurePartSet(param1);

        if (partSet == null)
        {
            return false;
        }

        string gender = partSet.Gender.ToUpperInvariant();
        return gender == "U" || gender.Equals(param2, StringComparison.InvariantCultureIgnoreCase);
    }

    /// @see class_1808.as::getFigureStringWithFigureIds
    public string GetFigureStringWithFigureIds(string param1, string param2, int[] param3)
    {
        // TODO(as3-port): Requires FigureDataContainer from com.sulake.habbo.utils.
        // AS3: creates FigureDataContainer, loads avatar data, resolves figure sets, saves part data.

        return param1;
    }

    /// @see class_1808.as::getItemIds
    public string[]? GetItemIds()
    {
        return _structure?.GetItemIds();
    }

    /// @see class_1808.as::getAnimationManager
    public IAnimationManager? GetAnimationManager()
    {
        return _structure?.AnimationManager;
    }

    /// @see class_1808.as::getMandatoryAvatarPartSetIds
    public string[]? GetMandatoryAvatarPartSetIds(string param1, int param2)
    {
        return _structure?.GetMandatorySetTypeIds(param1, param2).ToArray();
    }

    /// @see class_1808.as::getAssetByName
    public IAsset? GetAssetByName(string param1)
    {
        return _aliasCollection?.GetAssetByName(param1);
    }

    /// @see class_1808.as::get assets (IAvatarRenderManager.Assets)
    IAssetLibrary IAvatarRenderManager.Assets => context.assets as IAssetLibrary ?? new AssetLibraryCollection("empty");

    /// @see class_1808.as::get mode
    public string? Mode { get; set; }

    /// @see class_1808.as::injectFigureData
    public void InjectFigureData(XElement param1)
    {
        _structure?.InjectFigureData(param1);
    }

    /// @see class_1808.as::resetAssetManager
    public void ResetAssetManager()
    {
        _aliasCollection?.Reset();
    }

    /// @see class_1808.as::resolveClubLevel
    public int ResolveClubLevel(IFigureContainer param1, string param2, string[]? param3 = null)
    {
        if (_structure == null)
        {
            return 0;
        }

        FigureSetData? figureData = _structure.FigureData;

        if (figureData == null)
        {
            return 0;
        }

        string[] partTypeIds = param1.GetPartTypeIds();
        int clubLevel = 0;

        foreach (string typeId in partTypeIds)
        {
            ISetType? setType = figureData.GetSetType(typeId);

            if (setType == null)
            {
                continue;
            }

            int partSetId = param1.GetPartSetId(typeId);
            IFigurePartSet? partSet = setType.GetPartSet(partSetId);

            if (partSet == null)
            {
                continue;
            }

            clubLevel = Math.Max(partSet.ClubLevel, clubLevel);

            IPalette? palette = figureData.GetPalette(setType.PaletteId);
            int[] colorIds = param1.GetPartColorIds(typeId);

            clubLevel = colorIds.Select(colorId => palette?.GetColor(colorId))
                                .OfType<IPartColor>()
                                .Select(color => color.ClubLevel)
                                .Prepend(clubLevel)
                                .Max();
        }

        string[] bodyPartIds = param3 ?? _structure.GetBodyPartsUnordered("full");

        foreach (string bodyPartId in bodyPartIds)
        {
            ISetType? setType = figureData.GetSetType(bodyPartId);
            if (setType == null)
            {
                continue;
            }

            if (Array.IndexOf(partTypeIds, bodyPartId) == -1)
            {
                clubLevel = Math.Max(setType.OptionalFromClubLevel(param2), clubLevel);
            }
        }

        return clubLevel;
    }

    /// @see class_1808.as::purgeAssets
    public void PurgeAssets()
    {
        _avatarAssetDownloadManager?.Purge();
    }

    /// Loads the full HabboAvatarActions.xml from data/layouts/.
    /// In AS3 this was downloaded from the server; we embed it as a project data file.
    private static XElement LoadActionsXml()
    {
        // Try Godot res:// path first, then fall back to filesystem
        string[] paths =
        [
            "res://data/layouts/HabboAvatarActions.xml",
            System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "data", "layouts", "HabboAvatarActions.xml"),
        ];

        foreach (string path in paths)
        {
            try
            {
                if (path.StartsWith("res://", StringComparison.Ordinal))
                {
                    if (!Godot.FileAccess.FileExists(path))
                    {
                        continue;
                    }

                    using FileAccess? file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);

                    if (file != null)
                    {
                        return XElement.Parse(file.GetAsText());
                    }
                }
                else if (System.IO.File.Exists(path))
                {
                    return XElement.Load(path);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"[AvatarRenderManager] Failed to load actions XML from {path}: {ex.Message}");
            }
        }

        Logger.Warn("[AvatarRenderManager] HabboAvatarActions.xml not found, using minimal fallback");

        return XElement.Parse(
            """<actions><action id="Default" precedence="1000" state="std" main="1" isdefault="1" geometrytype="vertical" activepartset="figure" assetpartdefinition="std"/></actions>"""
        );
    }

    /// Extracts XML content from an asset in the library.
    private static XElement? GetAssetXml(IAssetLibrary? lib, string name)
    {
        if (lib == null || !lib.HasAsset(name))
        {
            return null;
        }

        return lib.GetAssetByName(name)?.Content as XElement;
    }

    /// @see class_1808.as::validateAvatarFigure
    private bool ValidateAvatarFigure(AvatarFigureContainer figure, string gender)
    {
        bool changed = false;

        if (_structure == null)
        {
            Logger.Warn("[AvatarRenderManager] validateAvatarFigure: structure is null!");
            return true;
        }

        int clubLevel = 2;
        List<string>? mandatoryIds = _structure.GetMandatorySetTypeIds(gender, clubLevel);

        if (mandatoryIds == null || mandatoryIds.Count == 0)
        {
            return true;
        }

        FigureSetData? figureData = _structure.FigureData;

        if (figureData == null)
        {
            Logger.Warn("[AvatarRenderManager] validateAvatarFigure: figureData is null!");
            return true;
        }

        foreach (string partTypeId in mandatoryIds)
        {
            if (!figure.HasPartType(partTypeId))
            {
                // Missing mandatory part — add default
                IFigurePartSet? defaultPartSet = _structure.GetDefaultPartSet(partTypeId, gender);

                if (defaultPartSet == null)
                {
                    continue;
                }

                figure.UpdatePart(partTypeId, defaultPartSet.Id, [0]);

                changed = true;
            }
            else
            {
                // Part exists — validate it's a known set
                ISetType? setType = figureData.GetSetType(partTypeId);
                if (setType == null)
                {
                    Logger.Warn($"[AvatarRenderManager] validateAvatarFigure: setType is null for '{partTypeId}'!");
                    continue;
                }

                IFigurePartSet? partSet = setType.GetPartSet(figure.GetPartSetId(partTypeId));

                if (partSet != null)
                {
                    continue;
                }

                // Invalid set — replace with default
                IFigurePartSet? defaultPartSet = _structure.GetDefaultPartSet(partTypeId, gender);

                if (defaultPartSet == null)
                {
                    continue;
                }

                figure.UpdatePart(partTypeId, defaultPartSet.Id, [0]);

                changed = true;
            }
        }

        return !changed;
    }

    /// @see class_1808.as::get events (exposed for download managers)
    public EventDispatcherWrapper Events => events;
}
