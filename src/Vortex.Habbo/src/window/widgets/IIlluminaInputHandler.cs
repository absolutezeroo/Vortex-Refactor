// @see habbo/window/widgets/IIlluminaInputHandler.as

using Vortex.Core.Window;
using Vortex.Core.Window.Components;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/IIlluminaInputHandler.as
public interface IIlluminaInputHandler
{
    /// @see habbo/window/widgets/IIlluminaInputHandler.as::onInput
    void OnInput(IWidgetWindow widget, string message);
}
