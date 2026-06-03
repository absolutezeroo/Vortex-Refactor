// @see core/assets/loaders/AssetLoaderEvent.as

namespace Vortex.Core.Assets.Loaders;

/// @see core/assets/loaders/AssetLoaderEvent.as
/// Event data for asset loader state changes.
public class AssetLoaderEvent
{
    /// @see AssetLoaderEvent.as::ASSET_LOADER_EVENT_COMPLETE
    public const string ASSET_LOADER_EVENT_COMPLETE = "AssetLoaderEventComplete";

    /// @see AssetLoaderEvent.as::ASSET_LOADER_EVENT_PROGRESS
    public const string ASSET_LOADER_EVENT_PROGRESS = "AssetLoaderEventProgress";

    /// @see AssetLoaderEvent.as::ASSET_LOADER_EVENT_UNLOAD
    public const string ASSET_LOADER_EVENT_UNLOAD = "AssetLoaderEventUnload";

    /// @see AssetLoaderEvent.as::ASSET_LOADER_EVENT_STATUS
    public const string ASSET_LOADER_EVENT_STATUS = "AssetLoaderEventStatus";

    /// @see AssetLoaderEvent.as::ASSET_LOADER_EVENT_ERROR
    public const string ASSET_LOADER_EVENT_ERROR = "AssetLoaderEventError";

    /// @see AssetLoaderEvent.as::ASSET_LOADER_EVENT_OPEN
    public const string ASSET_LOADER_EVENT_OPEN = "AssetLoaderEventOpen";

    /// @see AssetLoaderEvent.as::AssetLoaderEvent
    public AssetLoaderEvent(string type, int status)
    {
        Type = type;
        Status = status;
    }

    public string Type { get; }

    /// @see AssetLoaderEvent.as::get status
    public int Status { get; }

    public override string ToString()
    {
        return $"AssetLoaderEvent(type={Type}, status={Status})";
    }
}
