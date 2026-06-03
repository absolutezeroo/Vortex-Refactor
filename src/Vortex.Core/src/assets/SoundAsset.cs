// @see core/assets/SoundAsset.as

using System.Xml.Linq;

using Godot;

namespace Vortex.Core.Assets;

/// @see core/assets/SoundAsset.as
/// Godot adaptation: wraps AudioStream instead of Flash Sound.
public class SoundAsset : IAsset
{
    private AudioStream? _content;

    /// @see SoundAsset.as::SoundAsset
    public SoundAsset(AssetTypeDeclaration? declaration = null, string? url = null)
    {
        Declaration = declaration;
        Url = url;
    }

    /// @see SoundAsset.as::get url
    public string? Url { get; private set; }

    /// @see SoundAsset.as::get content
    public object? Content => _content;

    /// @see SoundAsset.as::get disposed
    public bool disposed { get; private set; }

    public Rect2? Rectangle => null;

    /// @see SoundAsset.as::get declaration
    public AssetTypeDeclaration? Declaration { get; private set; }

    /// @see SoundAsset.as::dispose
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

    /// @see SoundAsset.as::setUnknownContent
    public void SetUnknownContent(object? content)
    {
        switch (content)
        {
            case AudioStream audio:
                _content = audio;
                return;
            case SoundAsset soundAsset:
                _content = soundAsset._content;
                return;
        }
    }

    /// @see SoundAsset.as::setFromOtherAsset
    public void SetFromOtherAsset(IAsset param1)
    {
        if (param1 is not SoundAsset other)
        {
            return;
        }

        _content = other._content;
    }

    /// @see SoundAsset.as::setParamsDesc
    public void SetParamsDesc(IEnumerable<XElement>? param1)
    {
        // No params for sound assets in AS3.
    }
}
