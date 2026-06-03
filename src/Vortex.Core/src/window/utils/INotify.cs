// @see core/window/utils/INotify.as

using System;

namespace Vortex.Core.Window.Utils;

/// @see core/window/utils/INotify.as
public interface INotify : IDisposable
{
    /// @see core/window/utils/INotify.as::get/set title
    string? Title { get; set; }

    /// @see core/window/utils/INotify.as::get/set summary
    string? Summary { get; set; }

    /// @see core/window/utils/INotify.as::get/set callback
    object? Callback { get; set; }
}
