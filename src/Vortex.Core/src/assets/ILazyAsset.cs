// @see core/assets/ILazyAsset.as

namespace Vortex.Core.Assets;

/// @see core/assets/ILazyAsset.as
/// Asset that supports deferred content preparation.
public interface ILazyAsset : IAsset
{
    /// @see ILazyAsset.as::prepareLazyContent
    void PrepareLazyContent();
}
