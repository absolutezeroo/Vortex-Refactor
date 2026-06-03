// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/AvatarFigureContainer.as

using System.Linq;

namespace Vortex.Habbo.Avatar;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/AvatarFigureContainer.as
public class AvatarFigureContainer : IFigureContainer
{
    private Dictionary<string, PartData> _parts;

    /// @see AvatarFigureContainer.as::AvatarFigureContainer
    public AvatarFigureContainer(string figureString)
    {
        _parts = new Dictionary<string, PartData>();
        ParseFigureString(figureString);
    }

    /// @see AvatarFigureContainer.as::getPartTypeIds
    public string[] GetPartTypeIds()
    {
        return GetParts().Keys.ToArray();
    }

    /// @see AvatarFigureContainer.as::hasPartType
    public bool HasPartType(string type)
    {
        return GetParts().ContainsKey(type);
    }

    /// @see AvatarFigureContainer.as::getPartSetId
    public int GetPartSetId(string type)
    {
        if (GetParts().TryGetValue(type, out PartData? data))
        {
            return data.SetId;
        }

        return 0;
    }

    /// @see AvatarFigureContainer.as::getPartColorIds
    public int[] GetPartColorIds(string type)
    {
        if (GetParts().TryGetValue(type, out PartData? data))
        {
            return data.ColorIds.ToArray();
        }

        return [];
    }

    /// @see AvatarFigureContainer.as::updatePart
    public void UpdatePart(string type, int setId, int[] colorIds)
    {
        Dictionary<string, PartData> parts = GetParts();
        parts.Remove(type);
        parts[type] = new PartData(type, setId, new List<int>(colorIds));
    }

    /// @see AvatarFigureContainer.as::removePart
    public void RemovePart(string type)
    {
        GetParts().Remove(type);
    }

    /// @see AvatarFigureContainer.as::getFigureString
    public string GetFigureString()
    {
        List<string> segments = new();
        foreach (string type in GetParts().Keys)
        {
            List<string> parts = new()
            {
                type, GetPartSetId(type).ToString(),
            };
            int[] colorIds = GetPartColorIds(type);

            parts.AddRange(colorIds.Select(colorId => colorId.ToString()));

            segments.Add(string.Join("-", parts));
        }
        return string.Join(".", segments);
    }

    /// @see AvatarFigureContainer.as::getParts
    private Dictionary<string, PartData> GetParts()
    {
        _parts ??= new Dictionary<string, PartData>();
        return _parts;
    }

    /// @see AvatarFigureContainer.as::parseFigureString
    private void ParseFigureString(string? figureString)
    {
        figureString ??= "";

        foreach (string segment in figureString.Split('.'))
        {
            string[] tokens = segment.Split('-');

            if (tokens.Length < 2)
            {
                continue;
            }

            string type = tokens[0];
            // AS3: parseInt returns 0 on malformed input; C# int.Parse throws
            if (!int.TryParse(tokens[1], out int setId))
            {
                setId = 0;
            }

            List<int> colorIds = new();

            for (int i = 2;
                 i < tokens.Length;
                 i++)
            {
                colorIds.Add(int.TryParse(tokens[i], out int cid) ? cid : 0);
            }

            UpdatePart(type, setId, colorIds.ToArray());
        }
    }

    /// Internal data holder matching AS3 Map structure with type/setid/colorids keys.
    private sealed class PartData(string type, int setId, List<int> colorIds)
    {
        public string Type { get; } = type;

        public int SetId { get; } = setId;

        public List<int> ColorIds { get; } = colorIds;
    }
}
