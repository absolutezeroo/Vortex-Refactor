// @see core/window/motion/Dispose.as

namespace Vortex.Core.Window.Motion;

/// <summary>
/// Disposes target window when ticked. Sets target to null after disposal.
/// </summary>
/// @see core/window/motion/Dispose.as
public class DisposeMotion : Motion
{
    /// @see Dispose.as::Dispose
    public DisposeMotion(IWindow target) : base(target) { }

    /// @see Dispose.as::tick — AS3 calls dispose(), not destroy()
    internal override void Tick(long currentTimeMs)
    {
        base.Tick(currentTimeMs);

        if (Target is not { disposed: false })
        {
            return;
        }

        // @see Dispose.as — AS3 calls dispose() directly, not destroy()
        if (Target is System.IDisposable disposable)
        {
            disposable.Dispose();
        }
        else
        {
            Target.Destroy();
        }

        Target = null;
    }
}
