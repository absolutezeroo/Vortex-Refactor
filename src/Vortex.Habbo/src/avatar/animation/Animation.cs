// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/Animation.as

using System.Linq;
using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Animation;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/Animation.as
public class Animation : IAnimation
{
    private static readonly List<AnimationLayerData> EMPTY_FRAME = [];
    private static readonly List<string> EMPTY_STRINGS = [];
    private static readonly List<AddDataContainer> EMPTY_ADD = [];

    private readonly string _description;
    private readonly List<List<AnimationLayerData>> _frames;
    private readonly List<ISpriteDataContainer>? _spriteData;
    private readonly List<string>? _removeData;
    private readonly List<AddDataContainer>? _addData;
    private readonly Dictionary<string, string>? _overriddenActions;
    private readonly Dictionary<string, List<List<AnimationLayerData>>>? _overrideFrames;

    /// @see Animation.as::Animation
    public Animation(AvatarStructure structure, XElement xml)
    {
        _frames = [];

        Id = (string?)xml.Attribute("name") ?? "";
        _description = (string?)xml.Attribute("desc") ?? Id;
        // AS3: Boolean(xml.@resetOnToggle) — any non-empty string is true, empty/"" is false
        string resetVal = (string?)xml.Attribute("resetOnToggle") ?? "";
        ResetOnToggle = resetVal.Length > 0;

        // Parse <sprite> elements
        if (xml.Element("sprite") != null)
        {
            _spriteData = [];
            foreach (XElement spriteEl in xml.Elements("sprite"))
            {
                _spriteData.Add(new SpriteDataContainer(this, spriteEl));
            }
        }

        // Parse <avatar> element
        XElement? avatarEl = xml.Element("avatar");

        if (avatarEl != null)
        {
            AvatarData = new AvatarDataContainer(avatarEl);
        }

        // Parse <direction> element
        XElement? directionEl = xml.Element("direction");

        if (directionEl != null)
        {
            DirectionData = new DirectionDataContainer(directionEl);
        }

        // Parse <remove> elements
        if (xml.Element("remove") != null)
        {
            _removeData = [];
            foreach (XElement removeEl in xml.Elements("remove"))
            {
                _removeData.Add((string?)removeEl.Attribute("id") ?? "");
            }
        }

        // Parse <add> elements
        if (xml.Element("add") != null)
        {
            _addData = [];
            foreach (XElement addEl in xml.Elements("add"))
            {
                _addData.Add(new AddDataContainer(addEl));
            }
        }

        // Parse <override> elements
        // AS3: param2.name_6 — likely <override> children
        if (xml.Element("override") != null)
        {
            _overrideFrames = new Dictionary<string, List<List<AnimationLayerData>>>();
            _overriddenActions = new Dictionary<string, string>();

            foreach (XElement overrideEl in xml.Elements("override"))
            {
                string name = (string?)overrideEl.Attribute("name") ?? "";
                string overrideName = (string?)overrideEl.Attribute("override") ?? "";
                _overriddenActions[overrideName] = name;

                List<List<AnimationLayerData>> overrideFrameList = new();
                ParseFrames(overrideFrameList, overrideEl.Elements("frame"), structure);
                _overrideFrames[name] = overrideFrameList;
            }
        }

        // Parse main <frame> elements
        ParseFrames(_frames, xml.Elements("frame"), structure);
    }

    /// @see Animation.as::parseFrames
    private static void ParseFrames
    (
        List<List<AnimationLayerData>> target,
        IEnumerable<XElement> frameElements,
        AvatarStructure structure
    )
    {
        foreach (XElement frameEl in frameElements)
        {
            int repeats = (int?)frameEl.Attribute("repeats") ?? 1;

            if (repeats < 1)
            {
                repeats = 1;
            }

            for (int i = 0;
                 i < repeats;
                 i++)
            {
                List<AnimationLayerData> layerList =
                (
                    from bodyPartEl in frameEl.Elements("bodypart")
                    let actionStr = (string?)bodyPartEl.Attribute("action") ?? ""
                    let actionDef = structure.GetActionDefinition(actionStr)
                    select new AnimationLayerData(bodyPartEl, AnimationLayerData.TYPE_BODYPART, actionDef)).ToList();
                layerList.AddRange(
                    from fxEl in frameEl.Elements("fx")
                    let actionStr = (string?)fxEl.Attribute("action") ?? ""
                    let actionDef = structure.GetActionDefinition(actionStr)
                    select new AnimationLayerData(fxEl, AnimationLayerData.TYPE_FX, actionDef));

                target.Add(layerList);
            }
        }
    }

    /// @see Animation.as::frameCount
    public int FrameCount(string? actionOverride = null)
    {
        if (actionOverride == null)
        {
            return _frames.Count;
        }

        if (_overrideFrames != null &&
            _overrideFrames.TryGetValue(actionOverride, out List<List<AnimationLayerData>>? overrideFrameList))
        {
            return overrideFrameList.Count;
        }

        return 0;
    }

    /// @see Animation.as::hasOverriddenActions
    public bool HasOverriddenActions()
    {
        return _overriddenActions is
        {
            Count: > 0,
        };
    }

    /// @see Animation.as::overriddenActionNames
    public IList<string>? OverriddenActionNames()
    {
        if (_overriddenActions == null)
        {
            return null;
        }

        return [.. _overriddenActions.Keys];
    }

    /// @see Animation.as::overridingAction
    public string? OverridingAction(string action)
    {
        if (_overriddenActions == null)
        {
            return null;
        }

        return _overriddenActions.GetValueOrDefault(action);
    }

    /// @see Animation.as::getFrame
    private List<AnimationLayerData> GetFrame(int frame, string? actionOverride = null)
    {
        if (actionOverride == null)
        {
            if (_frames.Count > 0)
            {
                return _frames[frame % _frames.Count];
            }
        }
        else if (_overrideFrames != null &&
                 _overrideFrames.TryGetValue(actionOverride, out List<List<AnimationLayerData>>? overrideFrameList))
        {
            if (overrideFrameList.Count > 0)
            {
                return overrideFrameList[frame % overrideFrameList.Count];
            }
        }

        return EMPTY_FRAME;
    }

    /// @see Animation.as::getAnimatedBodyPartIds
    public List<string> GetAnimatedBodyPartIds(int frame, string? actionOverride = null)
    {
        List<string> result = new();

        foreach (AnimationLayerData layer in GetFrame(frame, actionOverride))
        {
            switch (layer.Type)
            {
                case AnimationLayerData.TYPE_BODYPART:
                    result.Add(layer.Id);
                    break;
                case AnimationLayerData.TYPE_FX:
                    {
                        if (_addData != null)
                        {
                            result.AddRange(from addItem in _addData where addItem.Id == layer.Id select addItem.Align);
                        }
                        break;
                    }
            }
        }

        return result;
    }

    /// @see Animation.as::getLayerData
    public AnimationLayerData? GetLayerData(int frame, string partId, string? actionOverride = null)
    {
        foreach (AnimationLayerData layer in GetFrame(frame, actionOverride))
        {
            if (layer.Id == partId)
            {
                return layer;
            }

            if (layer.Type != AnimationLayerData.TYPE_FX || _addData == null)
            {
                continue;
            }

            if (_addData.Any(addItem => addItem.Align == partId && addItem.Id == layer.Id))
            {
                return layer;
            }
        }

        return null;
    }

    /// @see Animation.as::hasAvatarData
    public bool HasAvatarData()
    {
        return AvatarData != null;
    }

    /// @see Animation.as::hasDirectionData
    public bool HasDirectionData()
    {
        return DirectionData != null;
    }

    /// @see Animation.as::hasAddData
    public bool HasAddData()
    {
        return _addData != null;
    }

    /// @see Animation.as::getAddData
    public AddDataContainer? GetAddData(string id)
    {
        if (_addData == null)
        {
            return null;
        }

        return _addData.FirstOrDefault(item => item.Id == id);

    }

    /// @see Animation.as::get id
    public string Id { get; }

    /// @see Animation.as::get spriteData
    public IList<ISpriteDataContainer>? SpriteData => _spriteData;

    /// @see Animation.as::get avatarData
    public AvatarDataContainer? AvatarData { get; }

    /// @see Animation.as::get directionData
    public DirectionDataContainer? DirectionData { get; }

    /// @see Animation.as::get removeData
    public IList<string> RemoveData => _removeData ?? (IList<string>)EMPTY_STRINGS;

    /// @see Animation.as::get addData
    public IList<AddDataContainer> AddData => _addData ?? (IList<AddDataContainer>)EMPTY_ADD;

    /// @see Animation.as::toString
    public override string ToString()
    {
        return _description;
    }

    /// @see Animation.as::get resetOnToggle
    public bool ResetOnToggle { get; }
}
