// @see core/assets/class_3607.as

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Core.Assets;

/// @see core/assets/class_3607.as
/// Callback interface for receiving loaded assets.
public interface IAssetReceiver : IDisposable
{
    void ReceiveAsset(IAsset asset, string resolvedName);
}
