// @see WIN63-202111081545-75921380-Source-main/src/login/ImageLoaderEvent.as

using System;

using Godot;

namespace Vortex.Login;

/// @see WIN63-202111081545-75921380-Source-main/src/login/ImageLoaderEvent.as
public sealed class ImageLoaderEvent(TextureRect loader, string url) : EventArgs
{
    /// @see WIN63-202111081545-75921380-Source-main/src/login/ImageLoaderEvent.as::loader
    public TextureRect Loader { get; } = loader;

    /// @see WIN63-202111081545-75921380-Source-main/src/login/ImageLoaderEvent.as::url
    public string Url { get; } = url;
}
