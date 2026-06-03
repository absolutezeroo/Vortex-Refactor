// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/AvatarStructure.as

using System.Linq;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Runtime.Events;
using Vortex.Habbo.Avatar.Actions;
using Vortex.Habbo.Avatar.Animation;
using Vortex.Habbo.Avatar.Enum;
using Vortex.Habbo.Avatar.Geometry;
using Vortex.Habbo.Avatar.Structure;
using Vortex.Habbo.Avatar.Structure.Animation;
using Vortex.Habbo.Avatar.Structure.Figure;
using Vortex.Habbo.Avatar.Structure.Parts;

namespace Vortex.Habbo.Avatar;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/AvatarStructure.as
public class AvatarStructure
{
    private AvatarModelGeometry? _geometry;
    private AvatarActionManager? _actionManager;
    private PartSetsData? _partSetsData;
    private AnimationData? _animationData;
    private ActionDefinition? _defaultAction;
    private Dictionary<string, Dictionary<int, List<string>>>? _mandatorySetTypeIdsCache;
    private bool _disposed;

    public EventDispatcherWrapper Events { get; } = new();

    /// @see AvatarStructure.as::AvatarStructure
    public AvatarStructure(AvatarRenderManager renderManager)
    {
        RenderManager = renderManager;
        FigureData = new FigureSetData();
        _partSetsData = new PartSetsData();
        _animationData = new AnimationData();
        AnimationManager = new AnimationManager();
        _mandatorySetTypeIdsCache = new Dictionary<string, Dictionary<int, List<string>>>();
    }

    /// @see AvatarStructure.as::dispose
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        RenderManager = null;
        FigureData = null;
        _partSetsData = null;
        _animationData = null;
        _mandatorySetTypeIdsCache = null;
        Events.Dispose();
    }

    /// @see AvatarStructure.as::init
    public void Init()
    {
        _mandatorySetTypeIdsCache = new Dictionary<string, Dictionary<int, List<string>>>();
    }

    /// @see AvatarStructure.as::initGeometry
    public void InitGeometry(XElement? xml)
    {
        if (xml == null)
        {
            return;
        }

        _geometry = new AvatarModelGeometry(xml);
    }

    /// @see AvatarStructure.as::initActions
    public void InitActions(IAssetLibrary assets, XElement? xml)
    {
        if (xml == null)
        {
            return;
        }

        _actionManager = new AvatarActionManager(assets, xml);
        _defaultAction = _actionManager.GetDefaultAction();
    }

    /// @see AvatarStructure.as::updateActions
    public void UpdateActions(XElement xml)
    {
        _actionManager?.UpdateActions(xml);
        _defaultAction = _actionManager?.GetDefaultAction();
    }

    /// @see AvatarStructure.as::initPartSets
    public bool InitPartSets(XElement? xml)
    {
        if (xml == null)
        {
            return false;
        }

        if (_partSetsData!.Parse(xml))
        {
            PartDefinition? ri = _partSetsData.GetPartDefinition("ri");
            if (ri != null)
            {
                ri.AppendToFigure = true;
            }

            PartDefinition? li = _partSetsData.GetPartDefinition("li");
            if (li != null)
            {
                li.AppendToFigure = true;
            }

            return true;
        }

        return false;
    }

    /// @see AvatarStructure.as::initAnimation
    public bool InitAnimation(XElement? xml)
    {
        return xml != null && _animationData!.Parse(xml);
    }

    /// @see AvatarStructure.as::initFigureData
    public bool InitFigureData(XElement? xml)
    {
        return xml != null && FigureData!.Parse(xml);
    }

    /// @see AvatarStructure.as::injectFigureData
    public void InjectFigureData(XElement xml)
    {
        FigureData?.InjectXml(xml);
    }

    /// @see AvatarStructure.as::registerAnimations
    public void RegisterAnimations(AssetLibraryCollection assets, string prefix = "fx", int count = 200)
    {
        for (int i = 0;
             i < count;
             i++)
        {
            string assetName = prefix + i;

            if (!assets.HasAsset(assetName))
            {
                continue;
            }

            IAsset? asset = assets.GetAssetByName(assetName);

            if (asset?.Content is XElement xml)
            {
                AnimationManager?.RegisterAnimation(this, xml);
            }
        }
    }

    /// @see AvatarStructure.as::registerAnimation
    public void RegisterAnimation(XElement xml)
    {
        AnimationManager?.RegisterAnimation(this, xml);
    }

    /// @see AvatarStructure.as::getPartColor
    public IPartColor? GetPartColor(IFigureContainer figure, string partType, int colorIndex = 0)
    {
        int[]? colorIds = figure.GetPartColorIds(partType);

        if (colorIds == null || colorIds.Length <= colorIndex)
        {
            return null;
        }

        ISetType? setType = FigureData?.GetSetType(partType);
        if (setType == null)
        {
            return null;
        }

        IPalette? palette = FigureData?.GetPalette(setType.PaletteId);

        if (palette == null)
        {
            return null;
        }

        return palette.GetColor(colorIds[colorIndex]);
    }

    /// @see AvatarStructure.as::getBodyPartData
    public AnimationLayerData? GetBodyPartData(string animId, int frameCount, string bodyPartId)
    {
        return AnimationManager?.GetLayerData(animId, frameCount, bodyPartId);
    }

    /// @see AvatarStructure.as::getAnimation
    public IAnimation? GetAnimation(string id)
    {
        return AnimationManager?.GetAnimation(id);
    }

    /// @see AvatarStructure.as::getActionDefinition
    public ActionDefinition? GetActionDefinition(string id)
    {
        return _actionManager?.GetActionDefinition(id);
    }

    /// @see AvatarStructure.as::getActionDefinitionWithState
    public ActionDefinition? GetActionDefinitionWithState(string state)
    {
        return _actionManager?.GetActionDefinitionWithState(state);
    }

    /// @see AvatarStructure.as::isMainAvatarSet
    public bool IsMainAvatarSet(string setId)
    {
        return _geometry?.IsMainAvatarSet(setId) ?? false;
    }

    /// @see AvatarStructure.as::sortActions
    public IList<IActiveActionData> SortActions(IList<IActiveActionData> actions)
    {
        return _actionManager?.SortActions(actions) ?? actions;
    }

    /// @see AvatarStructure.as::maxFrames
    public int MaxFrames(IList<IActiveActionData> actions)
    {
        int maxCount = 0;

        foreach (IActiveActionData action in actions)
        {
            if (action.Definition != null)
            {
                maxCount = System.Math.Max(maxCount, _animationData?.GetFrameCount(action.Definition) ?? 0);
            }
        }

        return maxCount;
    }

    /// @see AvatarStructure.as::getMandatorySetTypeIds
    public List<string> GetMandatorySetTypeIds(string gender, int clubLevel)
    {
        _mandatorySetTypeIdsCache ??= new Dictionary<string, Dictionary<int, List<string>>>();

        if (!_mandatorySetTypeIdsCache.TryGetValue(gender, out Dictionary<int, List<string>>? byClub))
        {
            byClub = new Dictionary<int, List<string>>();
            _mandatorySetTypeIdsCache[gender] = byClub;
        }

        if (byClub.TryGetValue(clubLevel, out List<string>? cached))
        {
            return cached;
        }

        List<string> result = FigureData?.GetMandatorySetTypeIds(gender, clubLevel) ?? [];
        byClub[clubLevel] = result;

        return result;
    }

    /// @see AvatarStructure.as::getDefaultPartSet
    public IFigurePartSet? GetDefaultPartSet(string type, string gender)
    {
        return FigureData?.GetDefaultPartSet(type, gender);
    }

    /// @see AvatarStructure.as::getCanvasOffsets
    public double[]? GetCanvasOffsets(IList<IActiveActionData> actions, string scale, int direction)
    {
        return _actionManager?.GetCanvasOffsets(actions, scale, direction);
    }

    /// @see AvatarStructure.as::getCanvas
    public AvatarCanvas? GetCanvas(string scale, string geometryId)
    {
        return _geometry?.GetCanvas(scale, geometryId);
    }

    /// @see AvatarStructure.as::removeDynamicItems
    public void RemoveDynamicItems(IAvatarImage avatarImage)
    {
        _geometry?.RemoveDynamicItems(avatarImage);
    }

    /// @see AvatarStructure.as::getActiveBodyPartIds
    public string[] GetActiveBodyPartIds(IActiveActionData action, IAvatarImage avatarImage)
    {
        if (action.Definition == null || _geometry == null || _partSetsData == null)
        {
            return [];
        }

        List<string> animPartIds = [];
        List<string> result = new();
        string geometryType = action.Definition.GeometryType;

        if (action.Definition.IsAnimation)
        {
            string animKey = action.Definition.State + "." + action.ActionParameter;

            if (AnimationManager?.GetAnimation(animKey) is Vortex.Habbo.Avatar.Animation.Animation anim)
            {
                animPartIds = anim.GetAnimatedBodyPartIds(0, action.OverridingAction);

                // AS3: dynamic part injection via addData (effects that add geometry items)
                if (anim.HasAddData())
                {
                    foreach (AddDataContainer addData in anim.AddData)
                    {
                        GeometryBodyPart? bodyPart = _geometry.GetBodyPart(geometryType, addData.Align);

                        if (bodyPart == null)
                        {
                            continue;
                        }

                        // Add dynamic geometry item to body part
                        XElement itemXml = new("item",
                            new XAttribute("id", addData.Id),
                            new XAttribute("x", "0"), new XAttribute("y", "0"), new XAttribute("z", "0"),
                            new XAttribute("radius", "0.01"),
                            new XAttribute("nx", "0"), new XAttribute("ny", "0"), new XAttribute("nz", "-1"),
                            new XAttribute("double", "1"));

                        bodyPart.AddPart(itemXml, avatarImage);

                        // Register part definition
                        XElement partXml = new("part", new XAttribute("set-type", addData.Id));
                        PartDefinition? partDef = _partSetsData.AddPartDefinition(partXml);

                        if (partDef != null)
                        {
                            partDef.AppendToFigure = true;

                            if (string.IsNullOrEmpty(addData.Base))
                            {
                                partDef.StaticId = 1;
                            }
                        }

                        if (!result.Contains(bodyPart.Id))
                        {
                            result.Add(bodyPart.Id);
                        }
                    }
                }
            }

            // AS3: iterate animated body part IDs using getBodyPart (NOT getBodyPartOfItem)
            foreach (string partId in animPartIds)
            {
                GeometryBodyPart? bodyPart = _geometry.GetBodyPart(geometryType, partId);

                if (bodyPart != null && !result.Contains(bodyPart.Id))
                {
                    result.Add(bodyPart.Id);
                }
            }
        }
        else
        {
            // Non-animation: use getBodyPartOfItem with active parts
            List<string> activeParts = _partSetsData.GetActiveParts(action.Definition);

            foreach (string partId in activeParts)
            {
                GeometryBodyPart? bodyPart = _geometry.GetBodyPartOfItem(geometryType, partId, avatarImage);

                if (bodyPart != null && !result.Contains(bodyPart.Id))
                {
                    result.Add(bodyPart.Id);
                }
            }
        }

        return result.ToArray();
    }

    /// @see AvatarStructure.as::getBodyPartsUnordered
    public string[] GetBodyPartsUnordered(string setId)
    {
        return _geometry?.GetBodyPartIdsInAvatarSet(setId) ?? [];
    }

    /// @see AvatarStructure.as::getBodyParts
    public List<string> GetBodyParts(string setId, string geometryId, int angle)
    {
        if (_geometry == null)
        {
            return [];
        }

        int angleDegrees = AvatarDirectionAngle.ANGLES[angle];
        return _geometry.GetBodyPartsAtAngle(setId, angleDegrees, geometryId);
    }

    /// @see AvatarStructure.as::getFrameBodyPartOffset
    public (int X, int Y) GetFrameBodyPartOffset(IActiveActionData action, int direction, int frame, string bodyPartId)
    {
        if (action.Definition == null || _animationData == null)
        {
            return (0, 0);
        }

        AnimationAction? animAction = _animationData.GetAction(action.Definition);

        if (animAction != null)
        {
            Vector2I offset = animAction.GetFrameBodyPartOffset(direction, frame, bodyPartId);
            return (offset.X, offset.Y);
        }

        Vector2I defaultOffset = AnimationAction.DefaultOffset;
        return (defaultOffset.X, defaultOffset.Y);
    }

    /// @see AvatarStructure.as::getParts
    public List<AvatarImagePartContainer>? GetParts(
        string bodyPartId, IFigureContainer figure, IActiveActionData action,
        string geometryType, int direction, List<string> hiddenLayers, IAvatarImage avatarImage,
        Dictionary<string, string>? overrideMap = null)
    {
        if (action?.Definition == null || _geometry == null || _partSetsData == null ||
            FigureData == null || _animationData == null)
        {
            return null;
        }

        List<string> activeParts = _partSetsData.GetActiveParts(action.Definition);
        List<AvatarImagePartContainer> partContainers = new();
        AnimationFrame?[] defaultFrames = new AnimationFrame?[]
        {
            null,
        };
        AnimationAction? animAction = _animationData.GetAction(action.Definition);
        Animation.Animation? anim = null;

        // AS3 uses concat() (new array) — never mutate the caller's hiddenLayers
        hiddenLayers = [.. hiddenLayers];

        if (action.Definition.IsAnimation)
        {
            string animKey = action.Definition.State + "." + action.ActionParameter;
            anim = AnimationManager?.GetAnimation(animKey);

            if (anim != null)
            {
                int frameCount = anim.FrameCount(action.OverridingAction);
                defaultFrames = new AnimationFrame?[frameCount];

                foreach (string animPartId in anim.GetAnimatedBodyPartIds(0, action.OverridingAction))
                {
                    if (animPartId != bodyPartId)
                    {
                        continue;
                    }

                    GeometryBodyPart? bodyPart = _geometry.GetBodyPart(geometryType, animPartId);
                    if (bodyPart != null)
                    {
                        foreach (GeometryItem dynPart in bodyPart.GetDynamicParts(avatarImage))
                        {
                            activeParts.Add(dynPart.Id);
                        }
                    }
                }
            }
        }

        List<string> orderedParts = _geometry.GetParts(geometryType, bodyPartId, direction, activeParts, avatarImage);
        string[] figurePartTypeIds = figure.GetPartTypeIds();

        if (bodyPartId is "head" or "torso")
        {
            Logger.Info(
                $"[AvatarStructure.GetParts] bodyPart='{bodyPartId}', orderedParts=[{string.Join(",", orderedParts)}], figurePartTypes=[{string.Join(",", figurePartTypeIds)}], activeParts=[{string.Join(",", activeParts)}]");
        }

        foreach (string partTypeId in figurePartTypeIds)
        {
            if (overrideMap != null && overrideMap.ContainsKey(partTypeId))
            {
                continue;
            }

            int partSetId = figure.GetPartSetId(partTypeId);
            int[]? colorIds = figure.GetPartColorIds(partTypeId);
            ISetType? setType = FigureData.GetSetType(partTypeId);
            if (setType == null)
            {
                if (bodyPartId is "head" or "torso")
                {
                    Logger.Info($"[AvatarStructure.GetParts] SKIP: setType null for '{partTypeId}'");
                }
                continue;
            }

            IPalette? palette = FigureData.GetPalette(setType.PaletteId);
            if (palette == null)
            {
                if (bodyPartId is "head" or "torso")
                {
                    Logger.Info($"[AvatarStructure.GetParts] SKIP: palette null for paletteId={setType.PaletteId}, type='{partTypeId}'");
                }
                continue;
            }

            IFigurePartSet? partSet = setType.GetPartSet(partSetId);
            if (partSet == null)
            {
                if (bodyPartId is "head" or "torso")
                {
                    Logger.Info($"[AvatarStructure.GetParts] SKIP: partSet null for type='{partTypeId}', setId={partSetId}");
                }
                continue;
            }

            foreach (string hidden in partSet.HiddenLayers)
            {
                hiddenLayers.Add(hidden);
            }

            foreach (IFigurePart figurePart in partSet.Parts)
            {
                if (!orderedParts.Contains(figurePart.Type))
                {
                    continue;
                }

                AnimationFrame?[] frames;
                if (animAction != null)
                {
                    AnimationActionPart? actionPart = animAction.GetPart(figurePart.Type);
                    frames = actionPart != null ? actionPart.Frames.ToArray<AnimationFrame?>() : defaultFrames;
                }
                else
                {
                    frames = defaultFrames;
                }

                IActionDefinition? partActionDef = activeParts.Contains(figurePart.Type) ? action.Definition : _defaultAction;
                PartDefinition? partDef = _partSetsData.GetPartDefinition(figurePart.Type);
                string flippedType = partDef?.FlippedSetType ?? figurePart.Type;
                if (string.IsNullOrEmpty(flippedType))
                {
                    flippedType = figurePart.Type;
                }

                IPartColor? color = null;
                if (colorIds != null && figurePart.ColorLayerIndex > 0 && colorIds.Length > figurePart.ColorLayerIndex - 1)
                {
                    color = palette.GetColor(colorIds[figurePart.ColorLayerIndex - 1]);
                }

                partContainers.Add(new AvatarImagePartContainer(
                    bodyPartId, figurePart.Type, figurePart.Id.ToString(),
                    color, frames, partActionDef, figurePart.ColorLayerIndex > 0,
                    figurePart.PaletteMap, flippedType));
            }
        }

        List<AvatarImagePartContainer> result = new();

        foreach (string partType in orderedParts)
        {
            bool found = false;
            IPartColor? overrideColor = null;
            bool isOverridden = overrideMap != null && overrideMap.ContainsKey(partType);

            foreach (AvatarImagePartContainer container in partContainers)
            {
                if (container.PartType != partType)
                {
                    continue;
                }

                if (isOverridden)
                {
                    overrideColor = container.Color;
                }
                else
                {
                    found = true;
                    if (!hiddenLayers.Contains(partType))
                    {
                        result.Add(container);
                    }
                }
            }

            if (found)
            {
                continue;
            }

            if (isOverridden)
            {
                string overrideId = overrideMap![partType];
                AnimationFrame?[] frames;
                if (animAction != null)
                {
                    AnimationActionPart? actionPart = animAction.GetPart(partType);
                    frames = actionPart != null ? actionPart.Frames.ToArray<AnimationFrame?>() : defaultFrames;
                }
                else
                {
                    frames = defaultFrames;
                }

                result.Add(new AvatarImagePartContainer(
                    bodyPartId, partType, overrideId, overrideColor, frames,
                    action.Definition, overrideColor != null, -1, partType, false, 1f));
            }
            else if (activeParts.Contains(partType))
            {
                GeometryBodyPart? bodyPart = _geometry.GetBodyPartOfItem(geometryType, partType, avatarImage);
                if (bodyPart != null && bodyPartId == bodyPart.Id)
                {
                    PartDefinition? partDef = _partSetsData.GetPartDefinition(partType);
                    if (partDef is { AppendToFigure: true })
                    {
                        bool isBlendable = false;
                        float blendAlpha = 1f;
                        string itemId = !string.IsNullOrEmpty(action.ActionParameter) ? action.ActionParameter : "1";
                        if (partDef.HasStaticId())
                        {
                            itemId = partDef.StaticId.ToString();
                        }

                        if (anim != null)
                        {
                            AddDataContainer? addData = anim.GetAddData(partType);
                            if (addData != null)
                            {
                                isBlendable = addData.IsBlended;
                                blendAlpha = (float)addData.Blend;
                            }
                        }

                        AnimationFrame?[] frames;
                        if (animAction != null)
                        {
                            AnimationActionPart? actionPart = animAction.GetPart(partType);
                            frames = actionPart != null ? actionPart.Frames.ToArray<AnimationFrame?>() : defaultFrames;
                        }
                        else
                        {
                            frames = defaultFrames;
                        }

                        result.Add(new AvatarImagePartContainer(
                            bodyPartId, partType, itemId, null, frames,
                            action.Definition, false, -1, partType, isBlendable, blendAlpha));
                    }
                }
            }
        }

        return result;
    }

    /// @see AvatarStructure.as::get figureData
    public FigureSetData? FigureData { get; private set; }

    /// @see AvatarStructure.as::get animationManager
    public AnimationManager? AnimationManager { get; }

    /// @see AvatarStructure.as::get renderManager
    public AvatarRenderManager? RenderManager { get; private set; }

    /// @see AvatarStructure.as::getItemIds
    public string[] GetItemIds()
    {
        if (_actionManager == null)
        {
            return [];
        }

        ActionDefinition? carryItemDef = _actionManager.GetActionDefinition("CarryItem");

        if (carryItemDef == null)
        {
            return [];
        }

        return carryItemDef.Params.Keys.ToArray();
    }

    /// @see AvatarStructure.as::getPopulatedArray
    private static int[] GetPopulatedArray(int count)
    {
        int[] result = new int[count];

        for (int i = 0;
             i < count;
             i++)
        {
            result[i] = i;
        }

        return result;
    }
}
