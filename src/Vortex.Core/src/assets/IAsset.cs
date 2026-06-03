// @see core/assets/IAsset.as

using System.Xml.Linq;

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Core.Assets;

/// @see core/assets/IAsset.as
public interface IAsset : IDisposable
{
    string? Url { get; }

    object? Content { get; }

    Godot.Rect2? Rectangle { get; }

    /// @see IAsset.as::get declaration
    AssetTypeDeclaration? Declaration { get; }

    void SetUnknownContent(object? content);

    /// @see IAsset.as::setFromOtherAsset
    void SetFromOtherAsset(IAsset param1);

    /// @see IAsset.as::setParamsDesc
    void SetParamsDesc(IEnumerable<XElement>? param1);
}
