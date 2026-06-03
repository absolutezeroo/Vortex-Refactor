// @see core/assets/loaders/class_37.as

using System;

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Core.Assets.Loaders;

/// @see core/assets/loaders/class_37.as
/// Abstract base for asset loaders with retry logic and event dispatching.
public abstract class AssetLoaderBase : IDisposable
{
    /// @see class_37.as::NONE
    public const uint NONE = 0;

    /// @see class_37.as::IO_ERROR
    public const uint IO_ERROR = 1;

    /// @see class_37.as::const_113
    public const uint SECURITY_ERROR = 2;

    /// @see class_37.as::_status
    protected int _status;

    /// @see class_37.as::var_51 — retry count
    protected int _retryCount;

    /// @see class_37.as::var_1413 — max retries
    protected int _maxRetries = 2;

    /// @see class_37.as::var_412 — error code
    protected uint _errorCode;

    protected bool _disposed;

    public bool disposed => _disposed;

    public uint ErrorCode => _errorCode;

    /// Raised on loader state changes (complete, progress, error, etc.).
    public event Action<AssetLoaderEvent>? LoaderEvent;

    /// @see class_37.as::loadEventHandler
    protected void DispatchLoaderEvent(string eventType)
    {
        switch (eventType)
        {
            case "httpStatus":
                RaiseEvent(AssetLoaderEvent.ASSET_LOADER_EVENT_STATUS);
                break;
            case "complete":
                RaiseEvent(AssetLoaderEvent.ASSET_LOADER_EVENT_COMPLETE);
                break;
            case "unload":
                RaiseEvent(AssetLoaderEvent.ASSET_LOADER_EVENT_UNLOAD);
                break;
            case "open":
                RaiseEvent(AssetLoaderEvent.ASSET_LOADER_EVENT_OPEN);
                break;
            case "progress":
                RaiseEvent(AssetLoaderEvent.ASSET_LOADER_EVENT_PROGRESS);
                break;
            case "ioError":
                _errorCode = IO_ERROR;
                if (!Retry())
                {
                    RaiseEvent(AssetLoaderEvent.ASSET_LOADER_EVENT_ERROR);
                }
                break;
            case "securityError":
                _errorCode = SECURITY_ERROR;
                if (!Retry())
                {
                    RaiseEvent(AssetLoaderEvent.ASSET_LOADER_EVENT_ERROR);
                }
                break;
        }
    }

    protected void RaiseEvent(string type)
    {
        LoaderEvent?.Invoke(new AssetLoaderEvent(type, _status));
    }

    /// @see class_37.as::retry
    protected virtual bool Retry()
    {
        return false;
    }

    public virtual void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        LoaderEvent = null;
    }
}
