// @see habbo/window/widgets/IAvatarImageWidget.as

using Vortex.Core.Window;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/IAvatarImageWidget.as
public interface IAvatarImageWidget : IWidget
{
    string? Figure { get; set; }

    string? Scale { get; set; }

    bool OnlyHead { get; set; }

    bool Cropped { get; set; }

    int Direction { get; set; }

    int UserId { get; set; }
}
