// @see core/window/motion/Motion.as

namespace Vortex.Core.Window.Motion;

/// <summary>
/// Base class for all motion animations. Tracks running state, completion,
/// target window, and a tag identifier for lookup.
/// </summary>
/// @see core/window/motion/Motion.as
public class Motion
{
    protected IWindow? _target;
    protected bool _running;
    protected bool _complete = true;
    protected string? _tag;

    /// @see Motion.as::Motion
    public Motion(IWindow? target)
    {
        _target = target;
    }

    /// @see Motion.as::get running
    public virtual bool Running => _running && _target is { disposed: false };

    /// @see Motion.as::get complete
    public bool Complete => _complete;

    /// @see Motion.as::get/set target
    public IWindow? Target
    {
        get => _target;
        set => _target = value;
    }

    /// @see Motion.as::get/set tag
    public string? Tag
    {
        get => _tag;
        set => _tag = value;
    }

    /// @see Motion.as::start
    virtual internal void Start()
    {
        _running = true;
    }

    /// @see Motion.as::update
    virtual internal void Update(double progress) { }

    /// @see Motion.as::stop
    virtual internal void Stop()
    {
        _target = null;
        _running = false;
    }

    /// @see Motion.as::tick
    virtual internal void Tick(long currentTimeMs) { }
}
