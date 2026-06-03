// @see core/assets/UnknownAsset.as

using System.Xml.Linq;

namespace Vortex.Core.Assets;

/// @see core/assets/UnknownAsset.as
/// Fallback asset type — stores arbitrary object content.
public class UnknownAsset : IAsset
{
    /// @see UnknownAsset.as::UnknownAsset
    public UnknownAsset(AssetTypeDeclaration? declaration = null, string? url = null)
    {
        Declaration = declaration;
        Url = url;
    }

    /// @see UnknownAsset.as::get url
    public string? Url { get; private set; }

    /// @see UnknownAsset.as::get content
    public object? Content { get; private set; }

    /// @see UnknownAsset.as::get disposed
    public bool disposed { get; private set; }

    public Godot.Rect2? Rectangle => null;

    /// @see UnknownAsset.as::get declaration
    public AssetTypeDeclaration? Declaration { get; private set; }

    /// @see UnknownAsset.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        Content = null;
        Declaration = null;
        Url = null;
    }

    /// @see UnknownAsset.as::setUnknownContent
    public void SetUnknownContent(object? content)
    {
        Content = content;
    }

    /// @see UnknownAsset.as::setFromOtherAsset
    public void SetFromOtherAsset(IAsset param1)
    {
        Content = param1.Content;
    }

    /// @see UnknownAsset.as::setParamsDesc
    public void SetParamsDesc(IEnumerable<XElement>? param1)
    {
        // No params for unknown assets in AS3.
    }

    public override string ToString()
    {
        return $"{GetType().FullName}: {Content}";
    }
}
