// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as

using System;
using System.Linq;
using System.Text;

using Vortex.Core.Runtime.Events;
using Vortex.Core.Runtime.Exceptions;

namespace Vortex.Core.Runtime;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as
public class Component : IUnknown, ICoreConfiguration, IDisposable
{
	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::COMPONENT_EVENT_RUNNING
	public const string COMPONENT_EVENT_RUNNING = "COMPONENT_EVENT_RUNNING";

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::COMPONENT_EVENT_DISPOSING
	public const string COMPONENT_EVENT_DISPOSING = "COMPONENT_EVENT_DISPOSING";

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::COMPONENT_EVENT_WARNING
	public const string COMPONENT_EVENT_WARNING = "COMPONENT_EVENT_WARNING";

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::COMPONENT_EVENT_ERROR
	public const string COMPONENT_EVENT_ERROR = "COMPONENT_EVENT_ERROR";

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::COMPONENT_EVENT_DEBUG
	public const string COMPONENT_EVENT_DEBUG = "COMPONENT_EVENT_DEBUG";

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::COMPONENT_EVENT_UNLOCKED
	public const string COMPONENT_EVENT_UNLOCKED = "COMPONENT_EVENT_UNLOCKED";

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::COMPONENT_EVENT_REBOOT
	public const string COMPONENT_EVENT_REBOOT = "COMPONENT_EVENT_REBOOT";

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::INTERNAL_EVENT_UNLOCKED
	protected const string INTERNAL_EVENT_UNLOCKED = "_INTERNAL_EVENT_UNLOCKED";

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::COMPONENT_FLAG_NULL
	public const uint COMPONENT_FLAG_NULL = 0;

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::COMPONENT_FLAG_INTERFACE
	public const uint COMPONENT_FLAG_INTERFACE = 1;

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::COMPONENT_FLAG_CONTEXT
	public const uint COMPONENT_FLAG_CONTEXT = 2;

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::COMPONENT_FLAG_DISPOSABLE
	public const uint COMPONENT_FLAG_DISPOSABLE = 4;

	protected uint _flags;
	protected string _lastError = string.Empty;
	protected string _lastDebugMessage = string.Empty;
	protected string _lastWarningMessage = string.Empty;

	private readonly Dictionary<string, object?> _assetStorage = new(StringComparer.Ordinal);
	private readonly List<Action> _cleanupFunctions = [];
	private readonly Dictionary<string, string> _configurationProperties = new(StringComparer.Ordinal);
	private readonly List<string> _requiredDependencyIIDs = [];
	private InterfaceStructList? _interfaceStructList;
	private IContext? _context;
	private int _requiredDependenciesCount = 1;

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::Component
	public Component(IContext? param1 = null, uint param2 = 0, object? param3 = null)
	{
		_flags = param2;
		_interfaceStructList = new InterfaceStructList();
		events = new EventDispatcherWrapper();
		_context = param1 ?? this as IContext;
		assets = param3;

		if (_context == null)
		{
			throw new InvalidComponentException("IContext not provided to Component's constructor!");
		}

		if (dependencies.Count > 0)
		{
			Lock();
		}

		foreach (ComponentDependency dependency in dependencies)
		{
			if (dependency.isRequired)
			{
				_requiredDependencyIIDs.Add(GetIidName(dependency.identifier));
			}

			InjectDependency(dependency.identifier, dependency.dependencySetter, dependency.isRequired, dependency.eventListeners);
		}

		AllDependenciesRequested();

		if (locked && _requiredDependencyIIDs.Count > 0)
		{
			Logger.Warn(
				$"[Component] {GetType().Name} has {_requiredDependencyIIDs.Count} unresolved required deps: {string.Join(", ", _requiredDependencyIIDs)}"
			);
		}
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::getInterfaceStructList
	internal static InterfaceStructList? GetInterfaceStructList(Component param1)
	{
		return param1._interfaceStructList;
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::get locked
	public bool locked { get; private set; }

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::get disposed
	public bool disposed { get; private set; }

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::get context
	public IContext context => _context!;

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::get events
	public EventDispatcherWrapper events { get; }

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::get assets
	public object? assets { get; protected set; }

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::findAssetByName
	public object? FindAssetByName(string param1)
	{
		return _assetStorage.GetValueOrDefault(param1);
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::removeAsset
	public object? RemoveAsset(string param1)
	{
		return !_assetStorage.Remove(param1, out object? value) ? null : value;
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::hasAsset
	public bool HasAsset(string param1)
	{
		return _assetStorage.ContainsKey(param1);
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::injectDependency
	private void InjectDependency(IID param1, Action<object?>? param2, bool param3, IList<DependencyEventListener>? param4)
	{
		if (param3)
		{
			_requiredDependenciesCount++;
		}

		QueueInterface(param1, CreateQueueCallback(param2, param3, param4));
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::createQueueCallback
	private Action<IID, IUnknown?> CreateQueueCallback(Action<object?>? param1, bool param2, IList<DependencyEventListener>? param3)
	{
		return (param4, param5) =>
		{
			if (disposed || param5 == null)
			{
				return;
			}

			param1?.Invoke(param5);

			if (param3 != null && param5 is Component component)
			{
				foreach (DependencyEventListener listener in param3)
				{
					component.events.AddEventListener(listener.type, listener.callback);
				}
			}

			_cleanupFunctions.Add(CreateCleanupFunction(param4, param5, param1, param3));

			if (param2)
			{
				AllDependenciesRequested(GetIidName(param4));
			}
		};
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::createCleanupFunction
	private static Action CreateCleanupFunction(IID param1, IUnknown param2, Action<object?>? param3,
		IList<DependencyEventListener>? param4)
	{
		return () =>
		{
			if (param4 != null && param2 is Component component)
			{
				foreach (DependencyEventListener listener in param4)
				{
					component.events.RemoveEventListener(listener.type, listener.callback);
				}
			}

			param3?.Invoke(null);
			param2.Release(param1);
		};
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::allDependenciesRequested
	private void AllDependenciesRequested(string param1 = "")
	{
		_requiredDependenciesCount--;

		if (!string.IsNullOrEmpty(param1))
		{
			_requiredDependencyIIDs.Remove(param1);
		}

		if (_requiredDependenciesCount != 0)
		{
			return;
		}

		InitComponent();
		Unlock();
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::get allRequiredDependenciesInjected
	protected bool allRequiredDependenciesInjected => _requiredDependenciesCount == 0;

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::get dependencies
	protected virtual IList<ComponentDependency> dependencies => Array.Empty<ComponentDependency>();

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::initComponent
	protected virtual void InitComponent() { }

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::queueInterface
	public virtual IUnknown? QueueInterface(IID param1, Action<IID, IUnknown?>? param2 = null)
	{
		InterfaceStruct? interfaceStruct = _interfaceStructList?.GetStructByInterface(param1);

		if (interfaceStruct == null)
		{
			return _context != null && !ReferenceEquals(_context, this) ? _context.QueueInterface(param1, param2) : null;
		}

		if (disposed)
		{
			throw new ComponentDisposedException("Failed to queue interface through disposed Component \"" + GetType().FullName + "\"!");
		}

		if (locked)
		{
			return null;
		}

		interfaceStruct.Reserve();

		IUnknown? unknown = interfaceStruct.unknown;

		param2?.Invoke(param1, unknown);

		return unknown;
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::release
	public virtual uint Release(IID param1)
	{
		if (disposed)
		{
			return 0;
		}

		InterfaceStruct? interfaceStruct = _interfaceStructList?.GetStructByInterface(param1);

		if (interfaceStruct == null)
		{
			_lastError = "Attempting to release unknown interface: " + GetIidName(param1) + "!";

			throw new System.Exception(_lastError);
		}

		uint referenceCount = interfaceStruct.Release();

		if ((_flags & COMPONENT_FLAG_DISPOSABLE) == 0 || referenceCount != 0 || _interfaceStructList!.GetTotalReferenceCount() != 0)
		{
			return referenceCount;
		}

		_context?.DetachComponent(this);

		Dispose();

		return referenceCount;
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::dispose
	public virtual void Dispose()
	{
		if (disposed)
		{
			return;
		}

		foreach (Action cleanup in _cleanupFunctions.ToArray())
		{
			cleanup();
		}

		_cleanupFunctions.Clear();
		events.DispatchEvent(COMPONENT_EVENT_DISPOSING, this);
		events.Dispose();
		_interfaceStructList?.Dispose();
		_interfaceStructList = null;
		_assetStorage.Clear();
		assets = null;
		_context = null;
		_flags = 0;
		disposed = true;
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::purge
	public virtual void Purge() { }

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::lock
	protected void Lock()
	{
		if (!locked)
		{
			locked = true;
		}
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::unlock
	protected void Unlock()
	{
		if (!locked)
		{
			return;
		}

		locked = false;

		events.DispatchEvent(new LockEvent(INTERNAL_EVENT_UNLOCKED, this));
		// TODO(as3-parity): Port trace sink for unlock logging equivalent.
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::toString
	public override string ToString()
	{
		return "[component " + GetType().FullName + " refs: " + _flags + "]";
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::toXMLString
	public virtual string ToXmlString(uint param1 = 0)
	{
		string indent = new('\t', (int)param1);
		StringBuilder builder = new();

		builder.Append(indent).Append("<component class=\"").Append(GetType().FullName).Append("\">\n");

		if (_interfaceStructList != null)
		{
			for (uint i = 0;
				 i < _interfaceStructList.length;
				 i++)
			{
				InterfaceStruct? @struct = _interfaceStructList.GetStructByIndex(i);

				if (@struct != null)
				{
					builder.Append(indent)
						   .Append("\t<interface iid=\"")
						   .Append(@struct.iis)
						   .Append("\" refs=\"")
						   .Append(@struct.references)
						   .Append("\"/>\n");
				}
			}
		}

		builder.Append(indent).Append("</component>\n");

		return builder.ToString();
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::get requiredDependencyIids
	public IList<string> requiredDependencyIiDs => _requiredDependencyIIDs;

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::registerUpdateReceiver
	public void RegisterUpdateReceiver(IUpdateReceiver param1, uint param2)
	{
		if (!disposed)
		{
			_context?.RegisterUpdateReceiver(param1, param2);
		}
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::removeUpdateReceiver
	public void RemoveUpdateReceiver(IUpdateReceiver param1)
	{
		if (!disposed)
		{
			_context?.RemoveUpdateReceiver(param1);
		}
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::get flags
	public uint flags => _flags;

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreConfiguration.as::propertyExists
	public virtual bool PropertyExists(string param1)
	{
		ICoreConfiguration? rootConfig = _context?.configuration;

		if (rootConfig != null && !ReferenceEquals(rootConfig, this))
		{
			return rootConfig.PropertyExists(param1);
		}

		return _configurationProperties.ContainsKey(param1);
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreConfiguration.as::getProperty
	public virtual string GetProperty(string param1, IDictionary<string, string>? param2 = null)
	{
		ICoreConfiguration? rootConfig = _context?.configuration;

		if (rootConfig != null && !ReferenceEquals(rootConfig, this))
		{
			return rootConfig.GetProperty(param1, param2);
		}

		string value = _configurationProperties.TryGetValue(param1, out string? stored) ? stored : string.Empty;

		if (param2 == null || param2.Count == 0)
		{
			return value;
		}

		return param2.Aggregate(value, (current, pair) => current.Replace("%" + pair.Key + "%", pair.Value, StringComparison.Ordinal));
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreConfiguration.as::setProperty
	public virtual void SetProperty(string param1, string param2, bool param3 = false, bool param4 = false)
	{
		_ = param4;

		if (!param3 && _configurationProperties.ContainsKey(param1))
		{
			return;
		}

		_configurationProperties[param1] = param2;
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreConfiguration.as::getBoolean
	public virtual bool GetBoolean(string param1)
	{
		string value = GetProperty(param1);

		return bool.TryParse(value, out bool parsed) && parsed;
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreConfiguration.as::getInteger
	public virtual int GetInteger(string param1, int param2)
	{
		string value = GetProperty(param1);

		return int.TryParse(value, out int parsed) ? parsed : param2;
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreConfiguration.as::interpolate
	public virtual string? Interpolate(string param1)
	{
		ICoreConfiguration? rootConfig = _context?.configuration;

		if (rootConfig != null && !ReferenceEquals(rootConfig, this))
		{
			return rootConfig.Interpolate(param1);
		}

		return GetProperty(param1);
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreConfiguration.as::updateUrlProtocol
	public virtual string UpdateUrlProtocol(string param1)
	{
		if (string.IsNullOrWhiteSpace(param1))
		{
			return param1;
		}

		if (param1.StartsWith("//", StringComparison.Ordinal))
		{
			return "https:" + param1;
		}

		return param1;
	}

	/// @see WIN63-202407091256-704579380-Source-main/core/runtime/Component.as::registerInterface
	protected void RegisterInterface(IID param1, IUnknown? param2 = null)
	{
		if (_interfaceStructList == null)
		{
			return;
		}

		InterfaceStruct? existing = _interfaceStructList.GetStructByInterface(param1);

		if (existing != null)
		{
			return;
		}

		_interfaceStructList.Insert(new InterfaceStruct(param1, param2 ?? this));
	}

	private static string GetIidName(IID param1)
	{
		return param1.GetType().FullName ?? param1.GetType().Name;
	}
}
