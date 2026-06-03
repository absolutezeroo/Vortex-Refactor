// @see habbo/window/utils/ElementPointerHandler.as

using System;

namespace Vortex.Habbo.Window.Utils;

/// <summary>
/// Message handler for hint pointer events from the server.
/// Dispatches to HabboWindowManagerComponent's hint system.
/// </summary>
/// @see habbo/window/utils/ElementPointerHandler.as
public class ElementPointerHandler : IDisposable
{
    private HabboWindowManagerComponent? _windowManager;

    /// @see ElementPointerHandler.as::ElementPointerHandler
    public ElementPointerHandler(HabboWindowManagerComponent windowManager)
    {
        _windowManager = windowManager;

        // TODO(communication): Register ElementPointerMessageEvent when communication layer wiring is complete.
        // AS3: _windowManager.communication.addHabboConnectionMessageEvent(new ElementPointerMessageEvent(onElementPointerMessage));
    }

    /// @see ElementPointerHandler.as::onElementPointerMessage
    private void OnElementPointerMessage(object? messageEvent)
    {
        if (_windowManager == null)
        {
            return;
        }

        // TODO(communication): Extract parser key from ElementPointerMessageEvent.
        // AS3: var key = param1.getParser().key;
        // if (key == null || key == "") _windowManager.hideHint(); else _windowManager.showHint(key);
    }

    /// @see ElementPointerHandler.as::dispose
    public void Dispose()
    {
        // TODO(communication): Remove ElementPointerMessageEvent from communication layer.
        _windowManager = null;
        GC.SuppressFinalize(this);
    }
}
