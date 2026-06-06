// @see com.sulake.habbo.toolbar.extensions.purse.class_3491

using Vortex.Core.Window;

namespace Vortex.Habbo.Toolbar.Extensions.Purse;

/// @see com.sulake.habbo.toolbar.extensions.purse.class_3491
public interface ICurrencyIndicator
{
    /// @see class_3491.as::dispose
    void Dispose();

    /// @see class_3491.as::get window
    IWindowContainer? window { get; }

    /// @see class_3491.as::registerUpdateEvents
    void RegisterUpdateEvents(object? eventDispatcher);
}
