// @see core/assets/TextAsset.as

using System;
using System.Text;
using System.Xml.Linq;

namespace Vortex.Core.Assets;

/// @see core/assets/TextAsset.as
public class TextAsset : IAsset
{
    private string? _content = "";

    /// @see TextAsset.as::TextAsset
    public TextAsset(AssetTypeDeclaration? declaration = null, string? url = null)
    {
        Declaration = declaration;
        Url = url;
    }

    /// @see TextAsset.as::get url
    public string? Url { get; private set; }

    /// @see TextAsset.as::get content
    public object? Content => _content;

    /// @see TextAsset.as::get disposed
    public bool disposed { get; private set; }

    public Godot.Rect2? Rectangle => null;

    /// @see TextAsset.as::get declaration
    public AssetTypeDeclaration? Declaration { get; private set; }

    /// @see TextAsset.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        _content = null;
        Declaration = null;
        Url = null;
    }

    /// @see TextAsset.as::setUnknownContent
    public void SetUnknownContent(object? content)
    {
        switch (content)
        {
            case string str:
                _content = str;
                return;
            case byte[] bytes:
                _content = Encoding.UTF8.GetString(bytes);
                return;
            case TextAsset textAsset:
                _content = textAsset._content;
                return;
            default:
                _content = content?.ToString() ?? "";
                break;
        }
    }

    /// @see TextAsset.as::setFromOtherAsset
    public void SetFromOtherAsset(IAsset param1)
    {
        if (param1 is not TextAsset other)
        {
            throw new InvalidOperationException("Provided asset is not of type TextAsset!");
        }

        _content = other._content;
    }

    /// @see TextAsset.as::setParamsDesc
    public void SetParamsDesc(IEnumerable<XElement>? param1)
    {
        // No params for text assets in AS3.
    }
}
