// @see core/window/motion/Combo.as

namespace Vortex.Core.Window.Motion;

/// <summary>
/// Parallel motion composition. Runs all motions simultaneously.
/// Complete when all sub-motions are complete.
/// </summary>
/// @see core/window/motion/Combo.as
public class Combo : Motion
{
    private readonly List<Motion> _active;
    private readonly List<Motion> _completed;

    /// @see Combo.as::Combo
    public Combo(params Motion[] motions) : base(motions.Length > 0 ? motions[0].Target : null)
    {
        _active = new List<Motion>(motions);
        _completed = new List<Motion>();
    }

    /// @see Combo.as::start
    internal override void Start()
    {
        base.Start();

        foreach (Motion motion in _active)
        {
            motion.Start();
        }
    }

    /// @see Combo.as::tick
    internal override void Tick(long currentTimeMs)
    {
        base.Tick(currentTimeMs);

        // Remove completed motions from previous tick
        foreach (Motion done in _completed)
        {
            _active.Remove(done);

            if (done.Running)
            {
                done.Stop();
            }
        }

        _completed.Clear();

        // Tick active motions
        foreach (Motion motion in _active)
        {
            if (motion.Running)
            {
                motion.Tick(currentTimeMs);
            }

            if (motion.Complete)
            {
                _completed.Add(motion);
            }
        }

        if (_active.Count > 0)
        {
            // Find a valid target
            foreach (Motion motion in _active)
            {
                _target = motion.Target;

                if (_target is { disposed: false })
                {
                    break;
                }
            }

            _complete = false;
        }
        else
        {
            _complete = true;
        }
    }
}
