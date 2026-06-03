// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/geometry/AvatarModelGeometry.as

using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

using Vortex.Habbo.Avatar.Structure;

namespace Vortex.Habbo.Avatar.Geometry;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/geometry/AvatarModelGeometry.as
public class AvatarModelGeometry
{
    private readonly AvatarSet _avatarSet;
    private readonly Dictionary<string, Dictionary<string, GeometryBodyPart>> _bodyPartsByType;
    private readonly Dictionary<string, Dictionary<string, GeometryBodyPart>> _itemToBodyPart;
    private AvatarMatrix4x4 _transform;
    private readonly AvatarVector3D _camera;
    private readonly Dictionary<string, Dictionary<string, AvatarCanvas>> _canvases;

    /// @see AvatarModelGeometry.as::AvatarModelGeometry
    public AvatarModelGeometry(XElement xml)
    {
        _camera = new AvatarVector3D(0, 0, 10);
        _transform = new AvatarMatrix4x4();
        _bodyPartsByType = new Dictionary<string, Dictionary<string, GeometryBodyPart>>();
        _itemToBodyPart = new Dictionary<string, Dictionary<string, GeometryBodyPart>>();
        _canvases = new Dictionary<string, Dictionary<string, AvatarCanvas>>();

        // AS3: var_4073 = new AvatarSet(param1.avatarset[0])
        XElement? avatarSetXml = xml.Element("avatarset");
        _avatarSet = new AvatarSet(avatarSetXml!);

        // Parse <camera> element if present
        XElement? cameraXml = xml.Element("camera");

        if (cameraXml != null)
        {
            _camera.X = double.Parse(cameraXml.Element("x")?.Value ?? "0", CultureInfo.InvariantCulture);
            _camera.Y = double.Parse(cameraXml.Element("y")?.Value ?? "0", CultureInfo.InvariantCulture);
            _camera.Z = double.Parse(cameraXml.Element("z")?.Value ?? "0", CultureInfo.InvariantCulture);
        }

        // Parse <canvas> elements
        foreach (XElement canvasXml in xml.Elements("canvas"))
        {
            string scale = canvasXml.Attribute("scale")?.Value ?? "";
            Dictionary<string, AvatarCanvas> canvasDict = new();

            foreach (XElement geometryXml in canvasXml.Elements("geometry"))
            {
                AvatarCanvas canvas = new(geometryXml, scale);
                canvasDict[geometryXml.Attribute("id")?.Value ?? ""] = canvas;
            }

            _canvases[scale] = canvasDict;
        }

        // Parse <type> elements
        foreach (XElement typeXml in xml.Elements("type"))
        {
            string typeId = typeXml.Attribute("id")?.Value ?? "";
            Dictionary<string, GeometryBodyPart> bodyPartDict = new();
            Dictionary<string, GeometryBodyPart> itemLookup = new();

            foreach (XElement bodyPartXml in typeXml.Elements("bodypart"))
            {
                GeometryBodyPart bodyPart = new(bodyPartXml);
                bodyPartDict[bodyPartXml.Attribute("id")?.Value ?? ""] = bodyPart;

                // Build item→bodypart reverse lookup via getPartIds(null)
                foreach (string partId in bodyPart.GetPartIds(null))
                {
                    itemLookup[partId] = bodyPart;
                }
            }

            _bodyPartsByType[typeId] = bodyPartDict;
            _itemToBodyPart[typeId] = itemLookup;
        }
    }

    /// @see AvatarModelGeometry.as::removeDynamicItems
    public void RemoveDynamicItems(IAvatarImage avatarImage)
    {
        foreach (GeometryBodyPart bodyPart in _bodyPartsByType.Values.SelectMany(bodyParts => bodyParts.Values))
        {
            bodyPart.RemoveDynamicParts(avatarImage);
        }
    }

    /// @see AvatarModelGeometry.as::getBodyPartIdsInAvatarSet
    public string[] GetBodyPartIdsInAvatarSet(string setId)
    {
        AvatarSet? avatarSet = _avatarSet.FindAvatarSet(setId);

        if (avatarSet != null)
        {
            return avatarSet.GetBodyParts();
        }

        return Array.Empty<string>();
    }

    /// @see AvatarModelGeometry.as::isMainAvatarSet
    public bool IsMainAvatarSet(string setId)
    {
        AvatarSet? avatarSet = _avatarSet.FindAvatarSet(setId);

        return avatarSet is { IsMain: true };

    }

    /// @see AvatarModelGeometry.as::getCanvas
    public AvatarCanvas? GetCanvas(string scale, string geometryId)
    {
        if (!_canvases.TryGetValue(scale, out Dictionary<string, AvatarCanvas>? canvasDict))
        {
            return null;
        }

        canvasDict.TryGetValue(geometryId, out AvatarCanvas? canvas);

        return canvas;
    }

    /// @see AvatarModelGeometry.as::typeExists
    private bool TypeExists(string type)
    {
        return _bodyPartsByType.ContainsKey(type);
    }

    /// @see AvatarModelGeometry.as::hasBodyPart
    private bool HasBodyPart(string type, string id)
    {
        return TypeExists(type) && _bodyPartsByType[type].ContainsKey(id);
    }

    /// @see AvatarModelGeometry.as::getBodyPartIDs
    /// Note: AS3 source also ignores the type parameter (unused local); ported faithfully.
    private List<string> GetBodyPartIds(string type)
    {
        return _bodyPartsByType.Keys.ToList();
    }

    /// @see AvatarModelGeometry.as::getBodyPartsOfType
    private Dictionary<string, GeometryBodyPart> GetBodyPartsOfType(string type)
    {
        if (TypeExists(type))
        {
            return _bodyPartsByType[type];
        }

        return new Dictionary<string, GeometryBodyPart>();
    }

    /// @see AvatarModelGeometry.as::getBodyPart
    public GeometryBodyPart? GetBodyPart(string type, string id)
    {
        Dictionary<string, GeometryBodyPart> bodyParts = GetBodyPartsOfType(type);

        bodyParts.TryGetValue(id, out GeometryBodyPart? bodyPart);

        return bodyPart;
    }

    /// @see AvatarModelGeometry.as::getBodyPartOfItem
    public GeometryBodyPart? GetBodyPartOfItem(string type, string itemId, IAvatarImage? avatarImage)
    {
        if (!_itemToBodyPart.TryGetValue(type, out Dictionary<string, GeometryBodyPart>? itemLookup))
        {
            return null;
        }

        if (itemLookup.TryGetValue(itemId, out GeometryBodyPart? bodyPart))
        {
            return bodyPart;
        }

        // Fallback: linear search through all body parts of this type
        Dictionary<string, GeometryBodyPart> bodyParts = GetBodyPartsOfType(type);

        return bodyParts.Values.FirstOrDefault(bp => bp.HasPart(itemId, avatarImage));

    }

    /// @see AvatarModelGeometry.as::getBodyPartsInAvatarSet
    private List<GeometryBodyPart> GetBodyPartsInAvatarSet(Dictionary<string, GeometryBodyPart> bodyParts, string setId)
    {
        List<GeometryBodyPart> result = new();
        string[] partIds = GetBodyPartIdsInAvatarSet(setId);

        foreach (string partId in partIds)
        {
            if (bodyParts.TryGetValue(partId, out GeometryBodyPart? bodyPart) && bodyPart != null)
            {
                result.Add(bodyPart);
            }
        }

        return result;
    }

    /// @see AvatarModelGeometry.as::getBodyPartsAtAngle
    public List<string> GetBodyPartsAtAngle(string setId, int angle, string? geometryType)
    {
        if (geometryType == null)
        {
            Logger.Warn("[AvatarModelGeometry] ERROR: Geometry ID not found for action: ");

            return new List<string>();
        }

        Dictionary<string, GeometryBodyPart> bodyPartsOfType = GetBodyPartsOfType(geometryType);
        List<GeometryBodyPart> bodyPartsInSet = GetBodyPartsInAvatarSet(bodyPartsOfType, setId);
        List<(double Distance, GeometryBodyPart BodyPart)> distancePairs = new();

        _transform = AvatarMatrix4x4.GetYRotationMatrix(angle);

        foreach (GeometryBodyPart bodyPart in bodyPartsInSet)
        {
            bodyPart.ApplyTransform(_transform);

            double distance = bodyPart.GetDistance(_camera);

            distancePairs.Add((distance, bodyPart));
        }

        distancePairs.Sort(OrderByDistance);

        List<string> result = new();

        foreach ((double _, GeometryBodyPart bodyPart) in distancePairs)
        {
            result.Add(bodyPart.Id);
        }

        return result;
    }

    /// @see AvatarModelGeometry.as::getParts
    public List<string> GetParts(string type, string bodyPartId, int angle, object? param4, IAvatarImage? avatarImage)
    {
        if (!HasBodyPart(type, bodyPartId))
        {
            return new List<string>();
        }

        GeometryBodyPart bodyPart = GetBodyPartsOfType(type)[bodyPartId];
        _transform = AvatarMatrix4x4.GetYRotationMatrix(angle);

        return bodyPart.GetParts(_transform, _camera, param4, avatarImage);
    }

    /// @see AvatarModelGeometry.as::orderByDistance
    private static int OrderByDistance((double Distance, GeometryBodyPart BodyPart) a, (double Distance, GeometryBodyPart BodyPart) b)
    {
        if (a.Distance < b.Distance)
        {
            return -1;
        }

        if (a.Distance > b.Distance)
        {
            return 1;
        }

        return 0;
    }
}
