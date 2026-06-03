// @see core/assets/class_66.as

using Vortex.Core.Runtime;

namespace Vortex.Core.Assets;

/// @see core/assets/class_66.as
/// IUpdateReceiver that processes queued ILazyAsset instances one per frame.
/// Registers at priority 2 when items are pending, deregisters when queue drains.
public class LazyAssetProcessor : IUpdateReceiver
{
    /// @see class_66.as::var_81
    private Queue<ILazyAsset>? _pending = new();

    /// @see class_66.as::var_92 — whether registered as update receiver
    private bool _registered;

    /// Context used to register/deregister as update receiver.
    private readonly IContext? _context;

    /// @see class_66.as::class_66
    /// Godot adaptation: takes IContext for update receiver registration instead of
    /// AS3's global class_79.instance singleton.
    public LazyAssetProcessor(IContext? context = null)
    {
        _context = context;
    }

    public bool disposed { get; private set; }

    /// @see class_66.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        _context?.RemoveUpdateReceiver(this);
        _pending = null;
        _registered = false;
        disposed = true;
    }

    /// @see class_66.as::push
    public void Push(ILazyAsset param1)
    {
        if (param1 == null || _pending == null)
        {
            return;
        }

        _pending.Enqueue(param1);

        if (!_registered && _context != null)
        {
            _context.RegisterUpdateReceiver(this, 2);
            _registered = true;
        }
    }

    /// @see class_66.as::flush
    public void Flush()
    {
        if (_pending == null)
        {
            return;
        }

        while (_pending.Count > 0)
        {
            ILazyAsset asset = _pending.Dequeue();
            if (!asset.disposed)
            {
                asset.PrepareLazyContent();
            }
        }

        if (_registered && _context != null)
        {
            _context.RemoveUpdateReceiver(this);
            _registered = false;
        }
    }

    /// @see class_66.as::update
    public void Update(uint param1)
    {
        if (_pending == null || _pending.Count == 0)
        {
            if (_registered && _context != null)
            {
                _context.RemoveUpdateReceiver(this);
                _registered = false;
            }
            return;
        }

        ILazyAsset asset = _pending.Dequeue();
        if (!asset.disposed)
        {
            asset.PrepareLazyContent();
        }
    }
}
