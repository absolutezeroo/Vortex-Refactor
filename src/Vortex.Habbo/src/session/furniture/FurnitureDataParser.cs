// @see com.sulake.habbo.session.furniture.FurnitureDataParser

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Vortex.Core.Assets.Loaders;
using Vortex.Core.Logging;
using Vortex.Core.Runtime.Events;

namespace Vortex.Habbo.Session.Furniture;

/// @see com.sulake.habbo.session.furniture.FurnitureDataParser
public class FurnitureDataParser
{
    public const string READY = "FDP_furniture_data_ready";

    private readonly Dictionary<int, IFurnitureData> _floorItems;
    private readonly Dictionary<int, IFurnitureData> _wallItems;
    private readonly Dictionary<string, List<int>> _floorItemClassNames;
    private readonly Dictionary<string, List<int>> _wallItemClassNames;

    public FurnitureDataParser(
        Dictionary<int, IFurnitureData> floorItems,
        Dictionary<int, IFurnitureData> wallItems,
        Dictionary<string, List<int>> floorItemClassNames,
        Dictionary<string, List<int>> wallItemClassNames)
    {
        _floorItems = floorItems;
        _wallItems = wallItems;
        _floorItemClassNames = floorItemClassNames;
        _wallItemClassNames = wallItemClassNames;
        events = new EventDispatcherWrapper();
    }

    public EventDispatcherWrapper events { get; }

    public void Dispose()
    {
        events.Dispose();
    }

    /// @see FurnitureDataParser.as::loadData
    public void LoadData(string url, string? checksum = null, string? language = null)
    {
        _ = checksum;
        _ = language;

        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        TextFileLoader loader = new("text/plain", url);

        try
        {
            string? data = GetTextContent(loader.Content);

            if (string.IsNullOrEmpty(data))
            {
                Logger.Warn("Could not download furnidata definition");
                return;
            }

            ParseData(data);
        }
        finally
        {
            loader.Dispose();
        }
    }

    /// Entry point for data already fetched as a string (XML or Lingo format).
    public void ParseData(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        if (data[0] == '<')
        {
            ParseXmlFormat(data);
        }
        else
        {
            ParseLingoFormat(data);
        }
    }

    /// @see FurnitureDataParser.as::parseXmlFormat
    private void ParseXmlFormat(string data)
    {
        XElement? root;
        try
        {
            root = XElement.Parse(data);
        }
        catch
        {
            return;
        }

        XElement? roomItems = root.Element("roomitemtypes");
        if (roomItems != null)
        {
            foreach (XElement node in roomItems.Elements("furnitype"))
            {
                FurnitureData item = ParseFloorItem(node);
                StoreItem(item);
            }
        }

        XElement? wallItems = root.Element("wallitemtypes");
        if (wallItems != null)
        {
            foreach (XElement node in wallItems.Elements("furnitype"))
            {
                FurnitureData item = ParseWallItem(node);
                StoreItem(item);
            }
        }

        events.DispatchEvent(READY);
    }

    /// @see FurnitureDataParser.as::parseFloorItem
    private static FurnitureData ParseFloorItem(XElement node)
    {
        int id = (int?)node.Attribute("id") ?? 0;
        string className = (string?)node.Attribute("classname") ?? "";

        string[] parts = className.Split('*');
        string baseName = parts[0];
        int colourIndex = parts.Length > 1 ? int.Parse(parts[1]) : 0;
        bool hasIndexedColor = parts.Length > 1;

        List<int> colours = new();
        XElement? partColors = node.Element("partcolors");
        if (partColors != null)
        {
            foreach (XElement colorNode in partColors.Elements("color"))
            {
                string colorStr = colorNode.Value;
                if (colorStr.StartsWith('#'))
                {
                    colours.Add(Convert.ToInt32(colorStr[1..], 16));
                }
                else
                {
                    colours.Add(-int.Parse(colorStr));
                }
            }
        }

        return new FurnitureData(
            "s", id, className, baseName,
            (string?)node.Element("name") ?? "",
            (string?)node.Element("description") ?? "",
            (int?)node.Element("revision") ?? 0,
            (int?)node.Element("xdim") ?? 1,
            (int?)node.Element("ydim") ?? 1,
            0,
            colours.ToArray(), hasIndexedColor, colourIndex,
            (string?)node.Element("adurl") ?? "",
            (int?)node.Element("offerid") ?? -1,
            (string?)node.Element("buyout") == "1",
            (int?)node.Element("rentofferid") ?? -1,
            (string?)node.Element("rentbuyout") == "1",
            (string?)node.Element("bc") == "1",
            (string?)node.Element("customparams"),
            (int?)node.Element("specialtype") ?? 0,
            (string?)node.Element("canstandon") == "1",
            (string?)node.Element("cansiton") == "1",
            (string?)node.Element("canlayon") == "1",
            (string?)node.Element("excludeddynamic") == "1",
            (string?)node.Element("furniline") ?? "");
    }

    /// @see FurnitureDataParser.as::parseWallItem
    private static FurnitureData ParseWallItem(XElement node)
    {
        int id = (int?)node.Attribute("id") ?? 0;
        string className = (string?)node.Attribute("classname") ?? "";

        return new FurnitureData(
            "i", id, className, className,
            (string?)node.Element("name") ?? "",
            (string?)node.Element("description") ?? "",
            (int?)node.Element("revision") ?? 0,
            0, 0, 0,
            null, false, 0,
            (string?)node.Element("adurl") ?? "",
            (int?)node.Element("offerid") ?? -1,
            (string?)node.Element("buyout") == "1",
            (int?)node.Element("rentofferid") ?? -1,
            (string?)node.Element("rentbuyout") == "1",
            (string?)node.Element("bc") == "1",
            null,
            (int?)node.Element("specialtype") ?? 0,
            false, false, false,
            (string?)node.Element("excludeddynamic") == "1",
            (string?)node.Element("furniline") ?? "");
    }

    /// @see FurnitureDataParser.as::parseLingoFormat
    private void ParseLingoFormat(string data)
    {
        Regex lineRe = new(@"\n\r{1,}|\n{1,}|\r{1,}", RegexOptions.Multiline);
        Regex blockRe = new(@"\[+?((.)*?)\]", RegexOptions.Singleline);

        string[] lines = lineRe.Split(data);
        foreach (string line in lines)
        {
            foreach (Match blockMatch in blockRe.Matches(line))
            {
                string block = blockMatch.Value;
                block = Regex.Replace(block, @"\[+", "");
                block = Regex.Replace(block, @"\]+", "");

                List<string> fields = block.Split('"').ToList();
                RemovePattern(fields, ", ");
                RemovePattern(fields, ",");
                if (fields.Count > 0)
                {
                    fields.RemoveAt(0);
                }

                if (fields.Count > 0)
                {
                    fields.RemoveAt(fields.Count - 1);
                }

                if (fields.Count < 18)
                {
                    continue;
                }

                string type = fields[0];
                int id = int.Parse(fields[1]);
                string className = fields[2];
                string[] classParts = className.Split('*');
                string baseName = classParts[0];
                int colourIndex = classParts.Length > 1 ? int.Parse(classParts[1]) : 0;
                bool hasIndexedColor = classParts.Length > 1;

                int revision = int.Parse(fields[3]);
                int ydim = int.Parse(fields[4]);
                int xdim = int.Parse(fields[5]);
                int zdim = int.Parse(fields[6]);

                List<int> colours = new();
                foreach (string c in fields[7].Split(','))
                {
                    string cv = c.Trim();
                    if (cv.StartsWith('#'))
                    {
                        colours.Add(Convert.ToInt32(cv[1..], 16));
                    }
                    else if (!string.IsNullOrEmpty(cv))
                    {
                        colours.Add(-int.Parse(cv));
                    }
                }

                string localizedName = fields[8];
                string adUrl = fields[10];
                int purchaseOfferId = int.Parse(fields[11]);
                bool purchaseCouldBeUsedForBuyout = fields[12] == "true";
                int rentOfferId = int.Parse(fields[13]);
                bool rentCouldBeUsedForBuyout = fields[14] == "true";
                string? customParams = fields[15];
                int category = int.Parse(fields[16]);
                bool availableForBuildersClub = fields[17] == "true";

                bool excludedDynamic = false;
                bool canStandOn = false;
                bool canSitOn = false;
                bool canLayOn = false;

                if (type == "i")
                {
                    if (fields.Count >= 19)
                    {
                        excludedDynamic = fields[18] == "1";
                    }
                }
                else
                {
                    if (fields.Count >= 19)
                    {
                        canStandOn = fields[18] == "1";
                    }

                    if (fields.Count >= 20)
                    {
                        canSitOn = fields[19] == "1";
                    }

                    if (fields.Count >= 21)
                    {
                        canLayOn = fields[20] == "1";
                    }

                    if (fields.Count >= 22)
                    {
                        excludedDynamic = fields[21] == "1";
                    }
                }

                FurnitureData item = new FurnitureData(
                    type, id, className, baseName,
                    localizedName, "",
                    revision, xdim, ydim, zdim,
                    colours.ToArray(), hasIndexedColor, colourIndex,
                    adUrl, purchaseOfferId, purchaseCouldBeUsedForBuyout,
                    rentOfferId, rentCouldBeUsedForBuyout,
                    availableForBuildersClub, customParams,
                    category, canStandOn, canSitOn, canLayOn,
                    excludedDynamic, "");

                StoreItem(item);
            }
        }

        events.DispatchEvent(READY);
    }

    /// @see FurnitureDataParser.as::storeItem
    private void StoreItem(FurnitureData item)
    {
        if (item.type == "s")
        {
            _floorItems[item.id] = item;
        }
        else if (item.type == "i")
        {
            _wallItems[item.id] = item;
        }

        Dictionary<string, List<int>> classNameMap = item.type == "s" ? _floorItemClassNames : _wallItemClassNames;

        if (!classNameMap.TryGetValue(item.className, out List<int>? ids))
        {
            ids = new List<int>();
            classNameMap[item.className] = ids;
        }

        while (ids.Count <= item.colourIndex)
        {
            ids.Add(0);
        }

        ids[item.colourIndex] = item.id;
    }

    private static string? GetTextContent(object? content)
    {
        return content switch
        {
            string text => text,
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            _ => null,
        };
    }

    /// @see FurnitureDataParser.as::removePatternFrom
    private static void RemovePattern(List<string> list, string pattern)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] == pattern)
            {
                list.RemoveAt(i);
            }
        }
    }
}
