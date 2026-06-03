// @see core/window/utils/IRectLimiter.as

namespace Vortex.Core.Window.Utils;

/// @see core/window/utils/IRectLimiter.as
public interface IRectLimiter
{
    /// @see core/window/utils/IRectLimiter.as::get minWidth
    int MinWidth { get; set; }

    /// @see core/window/utils/IRectLimiter.as::get maxWidth
    int MaxWidth { get; set; }

    /// @see core/window/utils/IRectLimiter.as::get minHeight
    int MinHeight { get; set; }

    /// @see core/window/utils/IRectLimiter.as::get maxHeight
    int MaxHeight { get; set; }

    /// @see core/window/utils/IRectLimiter.as::get isEmpty
    bool IsEmpty { get; }

    /// @see core/window/utils/IRectLimiter.as::setEmpty
    void SetEmpty();

    /// @see core/window/utils/IRectLimiter.as::limit
    void Limit();
}
