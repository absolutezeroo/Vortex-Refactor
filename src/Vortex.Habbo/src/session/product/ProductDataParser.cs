// @see com.sulake.habbo.session.product.ProductDataParser

using System.Text.RegularExpressions;
using System.Xml.Linq;

using Vortex.Core.Runtime.Events;

namespace Vortex.Habbo.Session.Product;

/// @see com.sulake.habbo.session.product.ProductDataParser
public class ProductDataParser
{
    public const string READY = "PDP_product_data_ready";

    private readonly Dictionary<string, IProductData> _products;

    public ProductDataParser(Dictionary<string, IProductData> products)
    {
        _products = products;
        events = new EventDispatcherWrapper();
    }

    public EventDispatcherWrapper events { get; }

    public void Dispose()
    {
        events.Dispose();
    }

    /// @see ProductDataParser.as::constructor — HTTP load
    /// TODO(assets): Replace with Godot HttpRequest loading when networking is wired.
    /// Call ParseData(text) once the HTTP response arrives.
    public void LoadData(string url, string? checksum = null, string? language = null)
    {
        // TODO(assets): Initiate Godot HttpRequest to url, then call ParseData on response body.
        _ = url;
        _ = checksum;
        _ = language;
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

    /// @see ProductDataParser.as::parseXmlFormat
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

        foreach (XElement product in root.Elements("product"))
        {
            string code = (string?)product.Attribute("code") ?? "";
            string name = (string?)product.Element("name") ?? "";
            _products[code] = new ProductData(code, name);
        }

        events.DispatchEvent(READY);
    }

    /// @see ProductDataParser.as::parseLingoFormat
    private void ParseLingoFormat(string data)
    {
        Regex lineRe = new(@"\n\r{1,}|\n{1,}|\r{1,}", RegexOptions.Multiline);
        Regex blockRe = new(@"\[+?((.)*?)\]", RegexOptions.Singleline);

        data = Regex.Replace(data, "\"{1,}", "");
        string[] lines = lineRe.Split(data);

        foreach (string line in lines)
        {
            foreach (Match blockMatch in blockRe.Matches(line))
            {
                string block = blockMatch.Value;
                block = Regex.Replace(block, @"\[+", "");
                block = Regex.Replace(block, @"\]+", "");

                string[] parts = block.Split(',');
                if (parts.Length < 2)
                {
                    continue;
                }

                string code = parts[0].Trim();
                string name = parts[1].Trim();
                _products[code] = new ProductData(code, name);
            }
        }

        events.DispatchEvent(READY);
    }
}
