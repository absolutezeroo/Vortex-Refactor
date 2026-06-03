using System.Xml.Linq;

using Vortex.Habbo.Room.Object.Visualization.Data;

namespace Vortex.Habbo.Room.Object.Visualization.Pet;

/// @see com.sulake.habbo.room.object.visualization.pet.PetAnimationSizeData
public class PetAnimationSizeData(int layerCount, int angle) : AnimationSizeData(layerCount, angle)
{
    public const int NO_ANIMATION = -1;

    private Dictionary<string, int> _postures = new();
    private List<string> _postureKeys = new();
    private Dictionary<string, int> _gestures = new();
    private List<string> _gestureKeys = new();
    private string? _defaultPosture;

    /// @see PetAnimationSizeData.as::definePostures
    public bool DefinePostures(XElement xml)
    {
        if (xml == null)
        {
            return false;
        }

        string? defaultAttr = xml.Attribute("defaultPosture")?.Value;
        _defaultPosture = defaultAttr;

        foreach (XElement posture in xml.Elements("posture"))
        {
            string? id = posture.Attribute("id")?.Value;
            string? animIdStr = posture.Attribute("animationId")?.Value;

            if (id == null || animIdStr == null)
            {
                return false;
            }

            int animId = int.Parse(animIdStr);
            _postures[id] = animId;
            _postureKeys.Add(id);

            if (_defaultPosture == null)
            {
                _defaultPosture = id;
            }
        }

        if (_defaultPosture == null || !_postures.ContainsKey(_defaultPosture))
        {
            return false;
        }

        return true;
    }

    /// @see PetAnimationSizeData.as::defineGestures
    public bool DefineGestures(XElement? xml)
    {
        if (xml == null)
        {
            return true;
        }

        foreach (XElement gesture in xml.Elements("gesture"))
        {
            string? id = gesture.Attribute("id")?.Value;
            string? animIdStr = gesture.Attribute("animationId")?.Value;

            if (id == null || animIdStr == null)
            {
                return false;
            }

            int animId = int.Parse(animIdStr);
            _gestures[id] = animId;
            _gestureKeys.Add(id);
        }

        return true;
    }

    /// @see PetAnimationSizeData.as::getAnimationForPosture
    public int GetAnimationForPosture(string? posture)
    {
        if (posture == null || !_postures.ContainsKey(posture))
        {
            posture = _defaultPosture;
        }

        if (posture != null && _postures.TryGetValue(posture, out int animId))
        {
            return animId;
        }

        return NO_ANIMATION;
    }

    /// @see PetAnimationSizeData.as::getGestureDisabled
    public bool GetGestureDisabled(string? posture)
    {
        return posture == "ded";
    }

    /// @see PetAnimationSizeData.as::getAnimationForGesture
    public int GetAnimationForGesture(string? gesture)
    {
        if (gesture == null || !_gestures.TryGetValue(gesture, out int animId))
        {
            return NO_ANIMATION;
        }

        return animId;
    }

    /// @see PetAnimationSizeData.as::getPostureForAnimation
    public string? GetPostureForAnimation(int index, bool fallbackToDefault)
    {
        if (index >= 0 && index < _postureKeys.Count)
        {
            return _postureKeys[index];
        }

        return fallbackToDefault ? _defaultPosture : null;
    }

    /// @see PetAnimationSizeData.as::getGestureForAnimation
    public string? GetGestureForAnimation(int index)
    {
        if (index >= 0 && index < _gestureKeys.Count)
        {
            return _gestureKeys[index];
        }

        return null;
    }

    /// @see PetAnimationSizeData.as::getGestureForAnimationId
    public string? GetGestureForAnimationId(int animationId)
    {
        foreach (KeyValuePair<string, int> entry in _gestures)
        {
            if (entry.Value == animationId)
            {
                return entry.Key;
            }
        }

        return null;
    }

    public int GetPostureCount()
    {
        return _postures.Count;
    }

    public int GetGestureCount()
    {
        return _gestures.Count;
    }
}
