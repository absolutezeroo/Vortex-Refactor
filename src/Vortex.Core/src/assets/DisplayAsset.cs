// @see core/assets/DisplayAsset.as

using System;
using System.Xml.Linq;

using Godot;

namespace Vortex.Core.Assets;

/// @see core/assets/DisplayAsset.as
/// Godot adaptation: wraps PackedScene instead of Flash DisplayObject.
/// Flash SWF loading has no direct Godot equivalent — structural port for interface completeness.
public class DisplayAsset : IAsset
{
    protected string? _url;
    protected PackedScene? _content;
    protected bool _disposed;
    protected AssetTypeDeclaration? _declaration;

    /// @see DisplayAsset.as::DisplayAsset
    public DisplayAsset(AssetTypeDeclaration? declaration = null, string? url = null)
    {
        _declaration = declaration;
        _url = url;
    }

    /// @see DisplayAsset.as::get url
    public string? Url => _url;

    /// @see DisplayAsset.as::get content
    public object? Content => _content;

    /// @see DisplayAsset.as::get disposed
    public bool disposed => _disposed;

    public Rect2? Rectangle => null;

    /// @see DisplayAsset.as::get declaration
    public AssetTypeDeclaration? Declaration => _declaration;

    /// @see DisplayAsset.as::dispose
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _content = null;
        _declaration = null;
        _disposed = true;
        _url = null;
    }

    /// @see DisplayAsset.as::setUnknownContent
    public void SetUnknownContent(object? content)
    {
        switch (content)
        {
            case PackedScene scene:
                _content = scene;

                return;
            case DisplayAsset displayAsset:
                {
                    _content = displayAsset._content;
                    _declaration = displayAsset._declaration;

                    if (_content == null)
                    {
                        throw new InvalidOperationException("Failed to read content from DisplayAsset!");
                    }

                    break;
                }
        }
    }

    /// @see DisplayAsset.as::setFromOtherAsset
    public void SetFromOtherAsset(IAsset param1)
    {
        if (param1 is not DisplayAsset other)
        {
            throw new InvalidOperationException("Provided asset should be of type DisplayAsset!");
        }

        _content = other._content;
        _declaration = other._declaration;
    }

    /// @see DisplayAsset.as::setParamsDesc
    public void SetParamsDesc(IEnumerable<XElement>? param1)
    {
        // No params for display assets in AS3.
    }
}
