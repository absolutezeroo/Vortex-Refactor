// @see habbo/window/widgets/IRoomPreviewerWidget.as

using Godot;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/IRoomPreviewerWidget.as
public interface IRoomPreviewerWidget
{
    /// @see habbo/window/widgets/IRoomPreviewerWidget.as::scale
    int Scale { get; set; }

    /// @see habbo/window/widgets/IRoomPreviewerWidget.as::roomPreviewer
    object? RoomPreviewer { get; }

    /// @see habbo/window/widgets/IRoomPreviewerWidget.as::offsetX
    int OffsetX { get; set; }

    /// @see habbo/window/widgets/IRoomPreviewerWidget.as::offsetY
    int OffsetY { get; set; }

    /// @see habbo/window/widgets/IRoomPreviewerWidget.as::zoom
    int Zoom { get; set; }

    /// @see habbo/window/widgets/IRoomPreviewerWidget.as::showPreview
    void ShowPreview(Image bitmapData);
}
