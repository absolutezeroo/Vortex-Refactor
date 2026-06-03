// @see core/window/motion/Queue.as

namespace Vortex.Core.Window.Motion;

/// <summary>
/// Sequential motion composition. Runs motions one after another.
/// Auto-advances to next when current completes.
/// </summary>
/// @see core/window/motion/Queue.as
public class Queue : Motion
{
    private Motion? _current;
    private readonly List<Motion> _motions;

    /// @see Queue.as::Queue
    public Queue(params Motion[] motions) : base(motions.Length > 0 ? motions[0].Target : null)
    {
        _motions = new List<Motion>(motions);
        _current = _motions.Count > 0 ? _motions[0] : null;
        _complete = _current == null;
    }

    /// @see Queue.as::get running
    public override bool Running => _running && _current is { Running: true };

    /// @see Queue.as::start
    internal override void Start()
    {
        base.Start();
        _current?.Start();
    }

    /// @see Queue.as::update
    internal override void Update(double progress)
    {
        base.Update(progress);

        if (_current is { Running: true })
        {
            _current.Update(progress);
        }
    }

    /// @see Queue.as::stop
    internal override void Stop()
    {
        base.Stop();

        _current?.Stop();
    }

    /// @see Queue.as::tick
    internal override void Tick(long currentTimeMs)
    {
        base.Tick(currentTimeMs);

        if (_current == null)
        {
            return;
        }

        _current.Tick(currentTimeMs);

        if (!_current.Complete)
        {
            return;
        }

        _current.Stop();

        int index = _motions.IndexOf(_current);

        if (index < _motions.Count - 1)
        {
            _current = _motions[index + 1];
            _target = _current.Target;

            _current.Start();
        }
        else
        {
            _complete = true;
        }
    }
}
