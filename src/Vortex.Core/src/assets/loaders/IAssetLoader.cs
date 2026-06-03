// @see core/assets/loaders/class_36.as

using System;

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Core.Assets.Loaders;

/// @see core/assets/loaders/class_36.as
/// Asset loader interface. Godot adaptation: uses Action callbacks instead of Flash IEventDispatcher.
public interface IAssetLoader : IDisposable
{
    string? Url { get; }

    object? Content { get; }

    byte[]? Bytes { get; }

    string? MimeType { get; }

    uint BytesLoaded { get; }

    uint BytesTotal { get; }

    uint ErrorCode { get; }

    int Id { get; }

    void Load(string url);

    /// Event callback for loader state changes.
    event Action<AssetLoaderEvent>? LoaderEvent;
}
