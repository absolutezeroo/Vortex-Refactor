using System;
using System.Globalization;

namespace Vortex.Habbo.Avatar.Pets;

/// <summary>
/// Parses a pet figure string ("typeId paletteId hexColor [head] customCount l1 p1 c1 ...").
/// </summary>
/// @see com.sulake.habbo.avatar.pets.PetFigureData
public class PetFigureData
{
    public PetFigureData(string figure)
    {
        TypeId = GetTypeId(figure);
        PaletteId = GetPaletteId(figure);
        Color = GetColor(figure);
        HeadOnly = GetHeadOnly(figure);

        string[] customData = GetCustomData(figure);
        CustomLayerIds = GetCustomLayerIds(customData);
        CustomPartIds = GetCustomPartIds(customData);
        CustomPaletteIds = GetCustomPaletteIds(customData);

        CustomParts = new PetCustomPart[CustomLayerIds.Length];
        for (int i = 0; i < CustomLayerIds.Length; i++)
        {
            CustomParts[i] = new PetCustomPart(CustomLayerIds[i], CustomPartIds[i], CustomPaletteIds[i]);
        }
    }

    public int TypeId { get; }

    public int PaletteId { get; }

    public int Color { get; }

    public bool HeadOnly { get; }

    public int[] CustomLayerIds { get; }

    public int[] CustomPartIds { get; }

    public int[] CustomPaletteIds { get; }

    public PetCustomPart[] CustomParts { get; }

    public bool HasCustomParts => CustomLayerIds.Length > 0;

    public PetCustomPart? GetCustomPart(int layerId)
    {
        foreach (PetCustomPart part in CustomParts)
        {
            if (part.LayerId == layerId)
            {
                return part;
            }
        }
        return null;
    }

    public string FigureString
    {
        get
        {
            string result = $"{TypeId} {PaletteId} {Color:x}";
            result += $" {CustomParts.Length}";
            foreach (PetCustomPart part in CustomParts)
            {
                result += $" {part.LayerId} {part.PartId} {part.PaletteId}";
            }
            return result;
        }
    }

    private string[] GetCustomData(string figure)
    {
        if (figure == null)
        {
            return [];
        }

        string[] parts = figure.Split(' ');
        int offset = HeadOnly ? 1 : 0;
        int dataStart = 4 + offset;

        if (parts.Length > dataStart)
        {
            int countIndex = 3 + offset;
            int count = int.TryParse(parts[countIndex], out int c) ? c : 0;
            int end = Math.Min(dataStart + (count * 3), parts.Length);
            return parts[dataStart..end];
        }

        return [];
    }

    private static int[] GetCustomLayerIds(string[] data)
    {
        int[] result = new int[data.Length / 3];
        for (int i = 0, j = 0; i < data.Length; i += 3, j++)
        {
            result[j] = int.TryParse(data[i], out int v) ? v : 0;
        }
        return result;
    }

    private static int[] GetCustomPartIds(string[] data)
    {
        int[] result = new int[data.Length / 3];
        for (int i = 1, j = 0; i < data.Length; i += 3, j++)
        {
            result[j] = int.TryParse(data[i], out int v) ? v : 0;
        }
        return result;
    }

    private static int[] GetCustomPaletteIds(string[] data)
    {
        int[] result = new int[data.Length / 3];
        for (int i = 2, j = 0; i < data.Length; i += 3, j++)
        {
            result[j] = int.TryParse(data[i], out int v) ? v : 0;
        }
        return result;
    }

    private static int GetTypeId(string figure)
    {
        if (figure != null)
        {
            string[] parts = figure.Split(' ');
            if (parts.Length >= 1 && int.TryParse(parts[0], out int v))
            {
                return v;
            }
        }
        return 0;
    }

    private static int GetPaletteId(string figure)
    {
        if (figure != null)
        {
            string[] parts = figure.Split(' ');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int v))
            {
                return v;
            }
        }
        return 0;
    }

    private static int GetColor(string figure)
    {
        if (figure != null)
        {
            string[] parts = figure.Split(' ');
            if (parts.Length >= 3 && int.TryParse(parts[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int v))
            {
                return v;
            }
        }
        return 0xFFFFFF;
    }

    private static bool GetHeadOnly(string figure)
    {
        if (figure != null)
        {
            string[] parts = figure.Split(' ');
            if (parts.Length >= 4)
            {
                return parts[3] == "head";
            }
        }
        return false;
    }
}
