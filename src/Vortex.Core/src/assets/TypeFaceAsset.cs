// @see core/assets/TypeFaceAsset.as

using System;
using System.Xml.Linq;

using Godot;

namespace Vortex.Core.Assets;

/// @see core/assets/TypeFaceAsset.as
/// Godot adaptation: wraps Godot Font instead of Flash Font.
/// No global font registration needed in Godot (unlike Flash class_285.registerFont).
public class TypeFaceAsset : IAsset
{
    protected AssetTypeDeclaration? _declaration;
    protected Font? _content;
    protected bool _disposed;

    /// @see TypeFaceAsset.as::TypeFaceAsset
    public TypeFaceAsset(AssetTypeDeclaration? declaration = null, string? url = null)
    {
        _declaration = declaration;
    }

    /// @see TypeFaceAsset.as::get url
    public string? Url => null;

    /// @see TypeFaceAsset.as::get content
    public object? Content => _content;

    /// @see TypeFaceAsset.as::get disposed
    public bool disposed => _disposed;

    public Rect2? Rectangle => null;

    /// @see TypeFaceAsset.as::get declaration
    public AssetTypeDeclaration? Declaration => _declaration;

    /// @see TypeFaceAsset.as::dispose
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _declaration = null;
        _content = null;
        _disposed = true;
    }

    /// @see TypeFaceAsset.as::setUnknownContent
    /// Godot adaptation: accepts Font directly instead of Flash Class→registerFont pattern.
    public void SetUnknownContent(object? content)
    {
        if (content is Font font)
        {
            _content = font;
        }
    }

    /// @see TypeFaceAsset.as::setFromOtherAsset
    public void SetFromOtherAsset(IAsset param1)
    {
        if (param1 is not TypeFaceAsset other)
        {
            throw new InvalidOperationException("Provided asset should be of type TypeFaceAsset!");
        }

        _content = other._content;
    }

    /// @see TypeFaceAsset.as::setParamsDesc
    public void SetParamsDesc(IEnumerable<XElement>? param1)
    {
        // No params for typeface assets in AS3.
    }
}
