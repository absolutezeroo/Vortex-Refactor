// @see core/window/utils/class_3501.as

using Godot;

namespace Vortex.Core.Window.Utils;

/// @see core/window/utils/class_3501.as
public interface IBitmapDataContainer
{
    /// @see core/window/utils/class_3501.as::get bitmapData
    Image? BitmapData { get; }

    /// @see core/window/utils/class_3501.as::get/set pivotPoint
    uint PivotPoint { get; set; }

    /// @see core/window/utils/class_3501.as::get/set stretchedX
    bool StretchedX { get; set; }

    /// @see core/window/utils/class_3501.as::get/set stretchedY
    bool StretchedY { get; set; }

    /// @see core/window/utils/class_3501.as::get/set zoomX
    float ZoomX { get; set; }

    /// @see core/window/utils/class_3501.as::get/set zoomY
    float ZoomY { get; set; }

    /// @see core/window/utils/class_3501.as::get/set greyscale
    bool Greyscale { get; set; }

    /// @see core/window/utils/class_3501.as::get/set etchingColor
    uint EtchingColor { get; set; }

    /// @see core/window/utils/class_3501.as::get etchingPoint
    Vector2 EtchingPoint { get; }

    /// @see core/window/utils/class_3501.as::get/set fitSizeToContents
    bool FitSizeToContents { get; set; }

    /// @see core/window/utils/class_3501.as::get/set wrapX
    bool WrapX { get; set; }

    /// @see core/window/utils/class_3501.as::get/set wrapY
    bool WrapY { get; set; }

    /// @see core/window/utils/class_3501.as::get/set rotation
    float Rotation { get; set; }
}
