// @see core/assets/XmlAsset.as

using System;
using System.Text;
using System.Xml.Linq;

namespace Vortex.Core.Assets;

/// @see core/assets/XmlAsset.as
public class XmlAsset : ILazyAsset
{
    private object? _unknownContent;
    private XElement? _content;

    /// @see XmlAsset.as::XmlAsset
    public XmlAsset(AssetTypeDeclaration? declaration = null, string? url = null)
    {
        Declaration = declaration;
        Url = url;
    }

    /// @see XmlAsset.as::get url
    public string? Url { get; private set; }

    /// @see XmlAsset.as::get content
    public object? Content
    {
        get
        {
            if (_content == null)
            {
                PrepareLazyContent();
            }
            return _content;
        }
    }

    /// @see XmlAsset.as::get disposed
    public bool disposed { get; private set; }

    public Godot.Rect2? Rectangle => null;

    /// @see XmlAsset.as::get declaration
    public AssetTypeDeclaration? Declaration { get; private set; }

    /// @see XmlAsset.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        _content = null;
        _unknownContent = null;
        Declaration = null;
        Url = null;
    }

    /// @see XmlAsset.as::setUnknownContent
    public void SetUnknownContent(object? content)
    {
        _content = null;
        _unknownContent = content;
    }

    /// @see XmlAsset.as::prepareLazyContent
    public void PrepareLazyContent()
    {
        switch (_unknownContent)
        {
            case byte[] bytes:
                _content = XElement.Parse(Encoding.UTF8.GetString(bytes));
                return;
            case string str:
                _content = XElement.Parse(str);
                return;
            case XElement xml:
                _content = xml;
                return;
            case XmlAsset xmlAsset:
                _content = xmlAsset._content;
                return;
        }
    }

    /// @see XmlAsset.as::setFromOtherAsset
    public void SetFromOtherAsset(IAsset param1)
    {
        if (param1 is not XmlAsset other)
        {
            throw new InvalidOperationException("Provided asset is not of type XmlAsset!");
        }

        _content = other._content;
    }

    /// @see XmlAsset.as::setParamsDesc
    public void SetParamsDesc(IEnumerable<XElement>? param1)
    {
        // No params for XML assets in AS3.
    }

    public override string ToString()
    {
        return $"XmlAsset _url:{Url} _content:{_content} _unknown:{_unknownContent}";
    }
}
