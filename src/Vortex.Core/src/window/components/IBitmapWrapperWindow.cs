// @see core/window/components/IBitmapWrapperWindow.as

using Godot;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/IBitmapWrapperWindow.as
public interface IBitmapWrapperWindow
{
    /// @see core/window/components/IBitmapWrapperWindow.as::get/set bitmap
    Image? Bitmap { get; set; }

    /// @see core/window/components/IBitmapWrapperWindow.as::get/set bitmapAssetName
    string? BitmapAssetName { get; set; }

    /// @see core/window/components/IBitmapWrapperWindow.as::get/set disposesBitmap
    bool DisposesBitmap { get; set; }
}
