// @see core/window/utils/WindowParser.as

using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Window.Components;

namespace Vortex.Core.Window.Utils;

/// @see core/window/utils/WindowParser.as
public class WindowParser : IWindowParser, IDisposable
{
    public const string ELEMENT_TAG_EXCLUDE = "_EXCLUDE";
    public const string ELEMENT_TAG_INCLUDE = "_INCLUDE";
    public const string const_778 = "_TEMP";

    private const string LAYOUT = "layout";
    private const string NAME_4 = "window";
    private const string VARIABLES = "variables";
    private const string FILTERS = "filters";
    private const string NAME = "name";
    private const string STYLE = "style";
    private const string CONST_1078 = "dynamic_style";
    private const string PARAMS = "params";
    private const string TAGS = "tags";
    private const string X = "x";
    private const string CONST_789 = "y";
    private const string WIDTH = "width";
    private const string HEIGHT = "height";
    private const string VISIBLE = "visible";
    private const string CAPTION = "caption";
    private const string ID = "id";
    private const string BACKGROUND = "background";
    private const string BLEND = "blend";
    private const string CLIPPING = "clipping";
    private const string COLOR = "color";
    private const string THRESHOLD = "treshold";
    private const string CHILDREN = "children";
    private const string WIDTH_MIN = "width_min";
    private const string CONST_682 = "width_max";
    private const string HEIGHT_MIN = "height_min";
    private const string CONST_665 = "height_max";
    private const string TRUE = "true";
    private const string ZERO = "0";
    private const string VAR = "$";
    private const string COMMA = ",";
    private const string EMPTY = "";

    private static readonly Regex Const440 = new("^(\\s|\\n|\\r|\\t|\\v)*", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex Const445 = new("(\\s|\\n|\\r|\\t|\\v)*$", RegexOptions.Compiled);
    protected readonly Dictionary<string, XElement> _parsedLayoutCache;
    protected readonly Dictionary<string, uint> var_3177;
    protected readonly Dictionary<uint, string> var_3316;

    protected readonly Dictionary<string, uint> var_3452;
    protected readonly Dictionary<uint, string> var_3623;
    protected IWindowContext? _context;

    /// @see core/window/utils/WindowParser.as::WindowParser
    public WindowParser()
        : this(null)
    {
    }

    /// @see core/window/utils/WindowParser.as::WindowParser
    public WindowParser(IWindowContext? param1)
    {
        _context = param1;
        var_3452 = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
        var_3316 = new Dictionary<uint, string>();
        var_3177 = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
        var_3623 = new Dictionary<uint, string>();
        _parsedLayoutCache = new Dictionary<string, XElement>(StringComparer.Ordinal);

        TypeCodeTable.FillTables(var_3452, var_3316);
        ParamCodeTable.FillTables(var_3177, var_3623);

        var_3452.TryAdd(NAME_4, 0);
        var_3316.TryAdd(0, NAME_4);
    }

    /// @see core/window/utils/WindowParser.as::get disposed
    public bool disposed { get; private set; }

    /// @see core/window/utils/WindowParser.as::trimWhiteSpace
    private static string TrimWhiteSpace(string param1)
    {
        param1 = Const445.Replace(param1, string.Empty);

        return Const440.Replace(param1, string.Empty);
    }

    /// @see core/window/utils/WindowParser.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        var_3452.Clear();
        var_3316.Clear();
        var_3177.Clear();
        var_3623.Clear();
        _parsedLayoutCache.Clear();
        _context = null;
        disposed = true;
    }

    /// @see core/window/utils/WindowParser.as::parseAndConstruct
    public IWindow? ParseAndConstruct(XElement param1, IWindow? param2 = null, Dictionary<string, object?>? param3 = null)
    {
        if (_context == null)
        {
            return null;
        }

        if (string.Equals(param1.Name.LocalName, LAYOUT, StringComparison.OrdinalIgnoreCase))
        {
            List<XElement>? variableNodes = param1.Element(VARIABLES)?.Elements().ToList();

            if (variableNodes is { Count: > 0 })
            {
                param3 ??= new Dictionary<string, object?>(StringComparer.Ordinal);

                foreach (XElement variable in variableNodes)
                {
                    string? key = variable.Attribute(NAME)?.Value;

                    if (!string.IsNullOrEmpty(key))
                    {
                        param3[key] = variable.Attribute("value")?.Value ?? variable.Value;
                    }
                }
            }

            // @see WindowParser.as::parseAndConstruct — layout-level filters applied to parent
            List<XElement>? layoutFilters = param1.Element(FILTERS)?.Elements().ToList();

            if (layoutFilters is { Count: > 0 } && param2 is Window.WindowController layoutWc)
            {
                List<object> builtFilters = layoutFilters.Select(BuildBitmapFilter).OfType<object>().ToList();

                if (builtFilters.Count > 0)
                {
                    layoutWc.filters = builtFilters;
                }
            }

            List<XElement> windowNodes = param1.Elements(NAME_4).ToList();

            switch (windowNodes.Count)
            {
                case 0:
                    return null;
                case 1:
                    param1 = windowNodes[0];
                    break;
                default:
                    {
                        IWindow? last = null;
                        foreach (XElement node in windowNodes)
                        {
                            last = ParseSingleWindowEntity(node, param2, param3);
                        }

                        return last;
                    }
            }
        }

        if (!string.Equals(param1.Name.LocalName, NAME_4, StringComparison.OrdinalIgnoreCase)
            || param1.HasAttributes)
        {
            return ParseSingleWindowEntity(param1, param2, param3);
        }

        List<XElement> children = param1.Elements().ToList();

        switch (children.Count)
        {
            case > 1:
                {
                    IWindow? last = null;
                    foreach (XElement child in children)
                    {
                        last = ParseSingleWindowEntity(child, param2, param3);
                    }

                    return last;
                }
            case 1:
                param1 = children[0];
                break;
            case 0:
                return null;
        }

        return ParseSingleWindowEntity(param1, param2, param3);
    }

    /// @see core/window/utils/WindowParser.as::parseSingleWindowEntity
    private IWindow? ParseSingleWindowEntity(XElement param1, IWindow? param2, Dictionary<string, object?>? param3)
    {
        if (_context == null)
        {
            return null;
        }

        string localName = param1.Name.LocalName;
        uint type = var_3452.GetValueOrDefault(localName, 0u);

        string name = Uri.UnescapeDataString(Convert.ToString(ParseAttribute(param1, NAME, param3, EMPTY)) ?? EMPTY);
        uint style = param2?.style ?? 0u;
        style = ParseUInt(ParseAttribute(param1, STYLE, param3, style));
        string dynamicStyle = Convert.ToString(ParseAttribute(param1, CONST_1078, param3, EMPTY)) ?? EMPTY;
        uint @params = ParseUInt(ParseAttribute(param1, PARAMS, param3, ZERO));
        string tagsRaw = Uri.UnescapeDataString(Convert.ToString(ParseAttribute(param1, TAGS, param3, EMPTY)) ?? EMPTY);

        Rect2 rect = new(
            ParseFloat(ParseAttribute(param1, X, param3, ZERO)),
            ParseFloat(ParseAttribute(param1, CONST_789, param3, ZERO)),
            ParseFloat(ParseAttribute(param1, WIDTH, param3, ZERO)),
            ParseFloat(ParseAttribute(param1, HEIGHT, param3, ZERO))
        );

        bool visible = string.Equals(Convert.ToString(ParseAttribute(param1, VISIBLE, param3, TRUE)), TRUE,
            StringComparison.OrdinalIgnoreCase);
        uint id = ParseUInt(ParseAttribute(param1, ID, param3, ZERO));

        // @see WindowParser.as::parseSingleWindowEntity — param bitmask OR from nested <params> children
        XElement? paramsNode = param1.Element(PARAMS);

        if (paramsNode != null)
        {
            List<XElement> paramChildren = paramsNode.Elements().ToList();

            foreach (string paramName in paramChildren.Select(paramChild =>
                         Convert.ToString(ParseAttribute(paramChild, NAME, param3, EMPTY)) ?? EMPTY))
            {
                if (!var_3177.TryGetValue(paramName, out uint paramBit))
                {
                    Logger.Warn($"Unknown window param: '{paramName}'");
                    continue;
                }

                @params |= paramBit;
            }
        }

        // @see WindowParser.as::parseSingleWindowEntity — caption default from parent if param 0x80000000 set
        string captionDefault = (@params & 0x80000000) != 0
            ? param2 != null ? param2.caption : EMPTY
            : EMPTY;
        captionDefault = Uri.UnescapeDataString(Convert.ToString(ParseAttribute(param1, CAPTION, param3, captionDefault)) ?? EMPTY);

        List<string>? tagList = null;

        if (!string.IsNullOrWhiteSpace(tagsRaw))
        {
            tagList = new List<string>(tagsRaw.Split(COMMA).Select(TrimWhiteSpace)
                                              .Where(static tag => !string.IsNullOrWhiteSpace(tag)));
        }

        IList<object> properties = ParseProperties(param1.Element(VARIABLES));

        // @see WindowParser.as — AS3 passes null for caption in create, null for parent if IIterable
        IWindow? parentForCreate = param2 is IIterable ? null : param2;
        IWindow? window = _context.Create(name, EMPTY, type, style, @params, rect, null, parentForCreate, id, properties, dynamicStyle,
            tagList);

        switch (window)
        {
            case null:
                return null;
            // @see WindowParser.as::parseSingleWindowEntity — apply limits
            case Window.WindowController
            {
                limits: not null,
            } wc:
                {
                    if (HasAttribute(param1, WIDTH_MIN))
                    {
                        wc.limits.MinWidth = (int)ParseFloat(ParseAttribute(param1, WIDTH_MIN, param3, wc.limits.MinWidth));
                    }

                    if (HasAttribute(param1, CONST_682))
                    {
                        wc.limits.MaxWidth = (int)ParseFloat(ParseAttribute(param1, CONST_682, param3, wc.limits.MaxWidth));
                    }

                    if (HasAttribute(param1, HEIGHT_MIN))
                    {
                        wc.limits.MinHeight =
                            (int)ParseFloat(ParseAttribute(param1, HEIGHT_MIN, param3, wc.limits.MinHeight));
                    }

                    if (HasAttribute(param1, CONST_665))
                    {
                        wc.limits.MaxHeight = (int)ParseFloat(ParseAttribute(param1, CONST_665, param3, wc.limits.MaxHeight));
                    }

                    wc.limits.Limit();
                    break;
                }
        }

        // @see WindowParser.as::parseSingleWindowEntity — apply attributes with defaults from created window
        bool background = string.Equals(
            Convert.ToString(ParseAttribute(param1, BACKGROUND, param3, window.background.ToString().ToLowerInvariant())),
            TRUE, StringComparison.OrdinalIgnoreCase
        );
        float blend = ParseFloat(ParseAttribute(param1, BLEND, param3, window.blend.ToString(CultureInfo.InvariantCulture)));
        bool clipping = string.Equals(
            Convert.ToString(ParseAttribute(param1, CLIPPING, param3, window.clipping.ToString().ToLowerInvariant())),
            TRUE, StringComparison.OrdinalIgnoreCase
        );
        string colorStr =
            Convert.ToString(ParseAttribute(param1, COLOR, param3, window.color.ToString(CultureInfo.InvariantCulture))) ?? "0";
        uint mouseThreshold =
            ParseUInt(ParseAttribute(param1, THRESHOLD, param3, window.mouseThreshold.ToString(CultureInfo.InvariantCulture)));

        // @see WindowParser.as — set only if different from defaults
        if (window.caption != captionDefault)
        {
            window.caption = captionDefault;
        }

        if (Math.Abs(window.blend - blend) > float.Epsilon)
        {
            window.blend = blend;
        }

        if (window.visible != visible)
        {
            window.visible = visible;
        }

        if (window.clipping != clipping)
        {
            window.clipping = clipping;
        }

        if (window.background != background)
        {
            window.background = background;
        }

        if (window.mouseThreshold != mouseThreshold)
        {
            window.mouseThreshold = mouseThreshold;
        }

        // @see WindowParser.as — parse color with hex support
        // AS3 parseInt truncates to 32-bit; values like "0xffff0e3f52" (>32 bits) are
        // truncated to the low 32 bits via ulong parse + cast.
        uint color;

        if (colorStr.Length > 1 && colorStr[1] == 'x')
        {
            if (ulong.TryParse(colorStr.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong colorLong))
            {
                color = (uint)(colorLong & 0xFFFFFFFF);
            }
            else
            {
                color = 0;
            }
        }
        else
        {
            color = ParseUInt(colorStr);
        }

        if (window.color != color)
        {
            window.color = color;
        }

        // @see WindowParser.as::parseSingleWindowEntity — parse filters
        XElement? filtersNode = param1.Element(FILTERS);

        if (filtersNode != null)
        {
            List<XElement> filterChildren = filtersNode.Elements().ToList();

            if (filterChildren.Count > 0 && window is Window.WindowController filterWc)
            {
                List<object> builtFilters = filterChildren.Select(BuildBitmapFilter).OfType<object>().ToList();

                if (builtFilters.Count > 0)
                {
                    filterWc.filters = builtFilters;
                }
            }
        }

        // @see WindowParser.as — IIterable parent handling
        if (param2 is IIterable iterable)
        {
            if (window.x != rect.Position.X || window.y != rect.Position.Y ||
                window.width != rect.Size.X || window.height != rect.Size.Y)
            {
                if ((@params & 192) == 192)
                {
                    window.x = rect.Position.X;
                }

                if ((@params & 3072) == 3072)
                {
                    window.y = rect.Position.Y;
                }
            }

            // @see WindowParser.as line 377-391 — AS3 uses IIterable(param2).iterator[iterator.length] = window
            // which goes through Proxy.setProperty → addChildAt on the iterator's backing container.
            IIterator? iterator = null;
            try
            {
                iterator = iterable.Iterator() as IIterator;
            }
            catch
            {
                /* @see WindowParser.as — swallows Error */
            }

            if (iterator != null)
            {
                iterator.SetProperty(iterator.Length, window);
            }
            else
            {
                param2.AddChild(window);
            }
        }

        // @see WindowParser.as — parse children
        XElement? childrenNode = param1.Element(CHILDREN);

        if (childrenNode != null)
        {
            List<XElement> childElements = childrenNode.Elements().ToList();

            // @see WindowParser.as — disable auto-rearrange during child parsing
            if (window is IClass3651 arrangeWindow)
            {
                arrangeWindow.SetAutoRearrange(false);
            }

            foreach (XElement child in childElements)
            {
                ParseAndConstruct(child, window, param3);
            }
        }

        // @see WindowParser.as — re-enable auto-rearrange after children
        if (window is IClass3651 arrangeWindowPost)
        {
            arrangeWindowPost.SetAutoRearrange(true);
        }

        return window;
    }

    /// @see core/window/utils/WindowParser.as::hasAttribute
    private static bool HasAttribute(XElement param1, string param2)
    {
        return param1.Attribute(param2) != null;
    }

    /// @see core/window/utils/WindowParser.as::parseAttribute
    private static object? ParseAttribute(XElement param1, string param2, Dictionary<string, object?>? param3, object? param4)
    {
        XAttribute? attribute = param1.Attribute(param2);
        if (attribute == null)
        {
            return param4;
        }

        string value = attribute.Value;

        if (param3 == null || !value.StartsWith(VAR, StringComparison.Ordinal))
        {
            return value;
        }

        string variableName = value[1..];

        if (!param3.TryGetValue(variableName, out object? sharedValue))
        {
            throw new Exception($"Shared variable not defined: \"{attribute.Value}\"!");
        }

        return sharedValue;
    }

    /// @see core/window/utils/WindowParser.as::parseProperties
    /// @see class_3540.as::parseVariableToken — handles nested Map/Array child elements
    private static IList<object> ParseProperties(XElement? param1)
    {
        if (param1 == null)
        {
            return new List<object>();
        }

        List<object> result = new();

        foreach (XElement variable in param1.Elements())
        {
            Dictionary<string, object?> dict = new(StringComparer.Ordinal);

            foreach (XAttribute attribute in variable.Attributes())
            {
                dict[attribute.Name.LocalName] = attribute.Value;
            }

            if (!dict.ContainsKey("name"))
            {
                dict["name"] = variable.Name.LocalName;
            }

            // @see class_3540.as::parseVariableToken — when no "value" attribute exists,
            // check for nested Map/Array child elements before falling back to text value.
            if (!dict.ContainsKey("value"))
            {
                string? typeStr = dict.TryGetValue("type", out object? t) ? t?.ToString() : null;

                if (typeStr is "Map" or "Array")
                {
                    // Store the XElement so XmlPropertyArrayParser can parse nested children
                    dict["_element"] = variable;
                }
                else
                {
                    // @see class_3540.as — check for child elements with Map/Array localName
                    XElement? firstChild = variable.Elements().FirstOrDefault();

                    if (firstChild != null)
                    {
                        string childName = firstChild.Name.LocalName;

                        if (childName is "Map" or "Array")
                        {
                            dict["type"] = childName;
                            dict["_element"] = firstChild;
                        }
                        else
                        {
                            dict["value"] = variable.Value;
                        }
                    }
                    else
                    {
                        dict["value"] = variable.Value;
                    }
                }
            }

            result.Add(dict);
        }

        return result;
    }

    /// @see core/window/utils/WindowParser.as::windowToXMLString
    public string WindowToXmlString(IWindow param1)
    {
        Window.WindowController? controller = param1 as Window.WindowController;

        // @see WindowParser.as — dynamicStyle handling
        if (controller != null)
        {
            if (controller._dynamicStyle.Length < 3)
            {
                controller._dynamicStyle = EMPTY;
            }

            if (controller._dynamicStyle != EMPTY)
            {
                param1.SetParamFlag(16, false);
            }
        }

        string nodeName = var_3316.GetValueOrDefault(param1.type, NAME_4);
        StringBuilder xml = new();

        xml.Append('<')
           .Append(nodeName)
           .Append(" x=\"")
           .Append(param1.x.ToString(CultureInfo.InvariantCulture))
           .Append("\"")
           .Append(" y=\"")
           .Append(param1.y.ToString(CultureInfo.InvariantCulture))
           .Append("\"")
           .Append(" width=\"")
           .Append(param1.width.ToString(CultureInfo.InvariantCulture))
           .Append("\"")
           .Append(" height=\"")
           .Append(param1.height.ToString(CultureInfo.InvariantCulture))
           .Append("\"")
           .Append(" params=\"")
           .Append(param1.param.ToString(CultureInfo.InvariantCulture))
           .Append("\"")
           .Append(" style=\"")
           .Append(param1.style.ToString(CultureInfo.InvariantCulture))
           .Append("\"");

        if (controller != null && controller._dynamicStyle != EMPTY)
        {
            xml.Append(" dynamic_style=\"").Append(controller._dynamicStyle).Append("\"");
        }

        if (!string.IsNullOrEmpty(param1.name))
        {
            xml.Append(" name=\"").Append(Uri.EscapeDataString(param1.name)).Append("\"");
        }

        if (!string.IsNullOrEmpty(param1.caption))
        {
            xml.Append(" caption=\"").Append(Uri.EscapeDataString(param1.caption)).Append("\"");
        }

        if (param1.id != 0)
        {
            xml.Append(" id=\"").Append(param1.id.ToString(CultureInfo.InvariantCulture)).Append("\"");
        }

        // @see WindowParser.as — color with hex format (if != 0xFFFFFF)
        if (param1.color != 0xFFFFFF)
        {
            xml.Append(" color=\"0x")
               .Append(param1.alpha.ToString("x", CultureInfo.InvariantCulture))
               .Append(param1.color.ToString("x", CultureInfo.InvariantCulture))
               .Append("\"");
        }

        if (Math.Abs(param1.blend - 1f) > float.Epsilon)
        {
            xml.Append(" blend=\"").Append(param1.blend.ToString(CultureInfo.InvariantCulture)).Append("\"");
        }

        if (!param1.visible)
        {
            xml.Append(" visible=\"false\"");
        }

        if (!param1.clipping)
        {
            xml.Append(" clipping=\"").Append(param1.clipping.ToString().ToLowerInvariant()).Append("\"");
        }

        if (param1.background)
        {
            xml.Append(" background=\"").Append(param1.background.ToString().ToLowerInvariant()).Append("\"");
        }

        if (param1.mouseThreshold != 10)
        {
            xml.Append(" treshold=\"").Append(param1.mouseThreshold.ToString(CultureInfo.InvariantCulture)).Append("\"");
        }

        if (param1.tags.Count > 0)
        {
            xml.Append(" tags=\"").Append(Uri.EscapeDataString(string.Join(COMMA, param1.tags))).Append("\"");
        }

        // @see WindowParser.as — limits serialization
        if (controller?.limits is { IsEmpty: false } limitsRef)
        {
            if (limitsRef.MinWidth > int.MinValue)
            {
                xml.Append(" width_min=\"").Append(limitsRef.MinWidth.ToString(CultureInfo.InvariantCulture)).Append("\"");
            }
            if (limitsRef.MaxWidth < int.MaxValue)
            {
                xml.Append(" width_max=\"").Append(limitsRef.MaxWidth.ToString(CultureInfo.InvariantCulture)).Append("\"");
            }
            if (limitsRef.MinHeight > int.MinValue)
            {
                xml.Append(" height_min=\"").Append(limitsRef.MinHeight.ToString(CultureInfo.InvariantCulture)).Append("\"");
            }
            if (limitsRef.MaxHeight < int.MaxValue)
            {
                xml.Append(" height_max=\"").Append(limitsRef.MaxHeight.ToString(CultureInfo.InvariantCulture)).Append("\"");
            }
        }

        xml.Append(">\r");

        // @see WindowParser.as — filters serialization
        if (controller?.filters is { Count: > 0 } filterList)
        {
            xml.Append("\t<filters>\r");
            foreach (object filter in filterList)
            {
                string? filterXml = FilterToXmlString(filter);
                if (filterXml != null)
                {
                    xml.Append("\t\t").Append(filterXml).Append("\r");
                }
            }
            xml.Append("\t</filters>\r");
        }

        StringBuilder childrenXml = new();

        // @see WindowParser.as — check if window is IIterable and iterate via iterator
        if (param1 is IIterable iterableWindow)
        {
            if (iterableWindow.Iterator() is IIterator iter)
            {
                for (uint i = 0; i < iter.Length; i++)
                {
                    if (iter.GetProperty(i) is IWindow iterChild && !iterChild.tags.Contains(ELEMENT_TAG_EXCLUDE))
                    {
                        childrenXml.Append(WindowToXmlString(iterChild));
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < param1.numChildren; i++)
            {
                IWindow? child = param1.GetChildAt(i);
                if (child != null && !child.tags.Contains(ELEMENT_TAG_EXCLUDE))
                {
                    childrenXml.Append(WindowToXmlString(child));
                }
            }
        }

        if (childrenXml.Length > 0)
        {
            xml.Append("\t<children>\r").Append(childrenXml).Append("\t</children>\r");
        }

        // @see WindowParser.as — serialize properties as <variables> child elements
        if (controller != null)
        {
            PropertyStruct[] props = controller.GetProperties();
            if (props.Length > 0)
            {
                xml.Append("\t<variables>\r");
                foreach (PropertyStruct prop in props)
                {
                    xml.Append("\t\t").Append(prop.ToXmlString()).Append("\r");
                }
                xml.Append("\t</variables>\r");
            }
        }

        xml.Append("</").Append(nodeName).Append(">\r");

        return xml.ToString();
    }

    /// @see core/window/utils/WindowParser.as::buildBitmapFilter
    private object? BuildBitmapFilter(XElement param1)
    {
        if (!string.Equals(param1.Name.LocalName, "DropShadowFilter", StringComparison.Ordinal))
        {
            return null;
        }

        Dictionary<string, string> attributes = new(StringComparer.Ordinal);

        foreach (XAttribute attribute in param1.Attributes())
        {
            attributes[attribute.Name.LocalName] = attribute.Value;
        }

        attributes["type"] = "DropShadowFilter";

        return attributes;
    }

    /// @see core/window/utils/WindowParser.as::filterToXmlString
    private static string? FilterToXmlString(object? param1)
    {
        if (param1 is not Dictionary<string, string> filter || !filter.TryGetValue("type", out string? type))
        {
            return null;
        }

        StringBuilder xml = new();

        xml.Append('<').Append(type);

        foreach (KeyValuePair<string, string> kv in filter.Where(kv => !string.Equals(kv.Key, "type", StringComparison.Ordinal)))
        {
            xml.Append(' ').Append(kv.Key).Append("=\"").Append(kv.Value).Append("\"");
        }

        xml.Append(" />");
        return xml.ToString();
    }

    private static float ParseFloat(object? value)
    {
        return value switch
        {
            null => 0f,
            float f => f,
            _ => float.TryParse(Convert.ToString(value), NumberStyles.Float, CultureInfo.InvariantCulture, out float result) ? result
                : 0f,
        };
    }

    private static uint ParseUInt(object? value)
    {
        if (value == null)
        {
            return 0;
        }

        return value switch
        {
            uint u => u,
            int i and >= 0 => (uint)i,
            long l and >= 0 => (uint)l,
            _ => uint.TryParse(Convert.ToString(value), NumberStyles.Integer, CultureInfo.InvariantCulture, out uint parsed) ? parsed
                : 0,
        };
    }
}
