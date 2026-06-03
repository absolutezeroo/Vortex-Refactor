// @see core/window/motion/Callback.as

using System;

namespace Vortex.Core.Window.Motion;

/// <summary>
/// Executes a callback function once when ticked. Null target.
/// Callback is called with the motion instance, then cleared.
/// </summary>
/// @see core/window/motion/Callback.as
public class Callback : Motion
{
    protected Action<Motion>? _callback;

    /// @see Callback.as::Callback
    public Callback(Action<Motion> callback) : base(null)
    {
        _callback = callback;
    }

    /// @see Callback.as::get running
    public override bool Running => _running && _callback != null;

    /// @see Callback.as::tick
    internal override void Tick(long currentTimeMs)
    {
        base.Tick(currentTimeMs);

        if (_callback == null)
        {
            return;
        }

        _callback(this);
        _callback = null;
    }
}
