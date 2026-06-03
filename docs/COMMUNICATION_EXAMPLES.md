# Communication Examples (C#)

## Mandatory Rule
For all new `Manager`, `Outgoing`, `Incoming`, and `Parser` classes, base implementation on the patterns below and on existing files in `src/Vortex.Habbo/communication/`.

Do not invent protocol or business behavior. Message fields, order, IDs, and flow must come from source of truth defined in `CONTEXT.md`. Only Godot/C# adaptation is allowed.

## Manager Pattern
Use a `Component`-based manager with dependency wiring and explicit dispose.

```csharp
public class ExampleCommunicationManager : Component
{
	private ICoreCommunicationManager? _communication;
	private IConnection? _connection;

	protected override IList<ComponentDependency> dependencies =>
		base.dependencies.Concat(
		[
			new ComponentDependency(new IIDCoreCommunicationManager(), x => _communication = x as ICoreCommunicationManager),
		]).ToList();

	protected override void InitComponent()
	{
		_connection = _communication?.CreateConnection(this);
		_connection?.RegisterMessageClasses(new HabboMessages());
	}
}
```

## Outgoing Pattern (Composer)
Implement `IMessageComposer`; keep payload order exactly as source.

```csharp
public class ExampleMessageComposer(string token) : IMessageComposer
{
	private readonly List<object> _payload = [token];
	public List<object> GetMessageArray() => _payload;
	public void Dispose() { }
}
```

## Incoming Pattern (Event)
Derive from `MessageEvent` and expose typed parser data via property accessors.

```csharp
public class ExampleMessageEvent(Action<IMessageEvent> cb)
	: MessageEvent(cb, typeof(ExampleMessageEventParser))
{
	public int value => ((ExampleMessageEventParser)parser!).value;
}
```

## Parser Pattern
Implement `Flush()` reset + `Parse()` sequential reads from `IMessageDataWrapper`.

```csharp
public class ExampleMessageEventParser : IMessageParser
{
	public int value { get; private set; } = -1;
	public bool Flush() { value = -1; return true; }
	public bool Parse(IMessageDataWrapper data)
	{
		value = data.ReadInteger();
		return true;
	}
}
```

## Registration Pattern
Register IDs in `src/Vortex.Habbo/communication/HabboMessages.cs`.

```csharp
events[2323] = typeof(AuthenticationOkMessageEvent);
composers[53] = typeof(SsoTicketMessageComposer);
```
