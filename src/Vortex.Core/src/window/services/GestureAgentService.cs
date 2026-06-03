// @see core/window/services/GestureAgentService.as

using System;

namespace Vortex.Core.Window.Services;

/// <summary>
/// Inertial scrolling/gesture service. Decays velocity by 0.75x every 40ms tick
/// until magnitude drops below 1, then stops.
/// Godot adaptation: uses frame-based timing instead of Flash Timer.
/// </summary>
/// @see core/window/services/GestureAgentService.as
public class GestureAgentService : IClass3739, IDisposable
{
	protected bool _active;
	protected IWindow? _window;
	protected uint _flags;
	protected Action<int, int>? _callback;
	protected double _velocityX;
	protected double _velocityY;
	private double _timerAccumulator;
	private const double TICK_INTERVAL = 0.04; // 40ms

	/// @see GestureAgentService.as::GestureAgentService
	public GestureAgentService() { }

	/// @see GestureAgentService.as::get disposed
	public bool Disposed { get; private set; }

	/// @see GestureAgentService.as::dispose
	public void DisposeService()
	{
		End(_window);
		Disposed = true;
	}

	void IDisposable.Dispose()
	{
		DisposeService();
		GC.SuppressFinalize(this);
	}

	/// @see GestureAgentService.as::begin
	public IWindow? Begin(IWindow? window, Action<int, int>? callback, uint flags, int velocityX, int velocityY)
	{
		_flags = flags;
		IWindow? previous = _window;

		if (_window != null)
		{
			End(_window);
		}

		if (window is not { disposed: false })
		{
			return previous;
		}

		_window = window;
		_callback = callback;
		_active = true;
		_velocityX = velocityX;
		_velocityY = velocityY;
		_timerAccumulator = 0;

		return previous;
	}

	/// <summary>
	/// Called each frame with delta time. Accumulates time and fires operate
	/// at 40ms intervals matching the AS3 Timer(40, 0) behavior.
	/// </summary>
	public void Update(double delta)
	{
		if (!_active)
		{
			return;
		}

		_timerAccumulator += delta;

		while (_timerAccumulator >= TICK_INTERVAL)
		{
			_timerAccumulator -= TICK_INTERVAL;
			Operate();
		}
	}

	/// @see GestureAgentService.as::operate
	protected void Operate()
	{
		_velocityX *= 0.75;
		_velocityY *= 0.75;

		if (Math.Abs(_velocityX) <= 1 && Math.Abs(_velocityY) <= 1)
		{
			End(_window);
		}
		else
		{
			_callback?.Invoke((int)_velocityX, (int)_velocityY);
		}
	}

	/// @see GestureAgentService.as::end
	public IWindow? End(IWindow? window)
	{
		IWindow? previous = _window;

		if (!_active || _window != window)
		{
			return previous;
		}

		_window = null;
		_active = false;
		_callback = null;

		return previous;
	}

	/// @see GestureAgentService.as::clientWindowDestroyed
	public void ClientWindowDestroyed()
	{
		End(_window);
	}

	public bool IsActive => _active;

	// IClass3739 explicit interface implementations

	IWindow? IClass3739.Begin(IWindow? window)
	{
		return Begin(window, null, 0, 0, 0);
	}
}
