// @see habbo/window/widgets/IBadgeImageWidget.as

using Vortex.Core.Window;
using Vortex.Core.Window.Utils;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/IBadgeImageWidget.as
/// AS3 extends class_3420 (IWidget) + class_3501 (IBitmapDataContainer)
public interface IBadgeImageWidget : IWidget, IBitmapDataContainer
{
    string? Type { get; set; }

    string? BadgeId { get; set; }

    int GroupId { get; set; }
}
