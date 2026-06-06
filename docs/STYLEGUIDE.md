# C# Style Guide

## Baseline
- Target stack: Godot 4.6 + C# (`net9.0`, nullable enabled).
- Use 4-space indentation and keep namespaces aligned with folder paths.
- Do not hand-edit Godot-generated `*.uid` files unless required.

## Naming
- Types, methods, properties: `PascalCase`
- Locals and parameters: `camelCase`
- Private fields: `_camelCase`
- Constants: `UPPER_SNAKE_CASE`

## File And Type Layout
- One public type per file.
- File name must match the primary public type.
- Keep `using` blocks minimal and sorted.
- `using` groups separated by blank lines: `System.*` first, then project namespaces.

## Breathing — Vertical Spacing

Code must be airy and scannable. Each logical unit is visually separated.

### Between members
- **One blank line** between every member (fields, properties, methods).
- **No blank lines** between fields in the same logical group (e.g., a cluster of related `_camelCase` fields).

### Within methods
- **Blank line after early-return guards:** `if (x == null) { return; }` is its own block, followed by a blank line.
- **Blank line between "get → check" pairs:** Fetch a value, blank line, then check/use it.
- **Blank line after collection/array initializers** before using the result.
- **Blank line between distinct logical operations:** get → process → dispatch are separate visual blocks.
- **Blank line before `switch`/`if` chains** when preceded by other statements.
- **Blank line between `case` blocks** in a `switch` when cases contain multiple statements.
- **No blank line** between tightly coupled one-liners (e.g., consecutive `model.SetNumber(...)` calls).

### Example (correct)
```csharp
public override void UseObject()
{
	if (Object == null)
	{
		return;
	}

	int state = Object.GetState(0);

	switch (state)
	{
		case NOT_STARTED:
			DispatchEvent(new RoomObjectWidgetRequestEvent(
				RoomObjectWidgetRequestEvent.ACHIEVEMENT_RESOLUTION_OPEN, Object));

			break;
		case IN_PROGRESS:
			break;
	}
}
```

### Anti-pattern (too compact)
```csharp
public override void UseObject()
{
	if (Object == null) { return; }
	int state = Object.GetState(0);
	switch (state)
	{
		case NOT_STARTED:
			DispatchEvent(new RoomObjectWidgetRequestEvent(
				RoomObjectWidgetRequestEvent.ACHIEVEMENT_RESOLUTION_OPEN, Object));
			break;
		case IN_PROGRESS:
			break;
	}
}
```

## Language Rules
- Always use braces for control blocks (`if/else/for/foreach/while`).
- Prefer explicit types when it improves readability; use `var` only when obvious.
- Keep nullable annotations accurate; avoid `!` unless invariant is documented.
- Async methods must end with `Async` and return `Task`/`Task<T>`.
- Avoid `async void` except for event handlers.
- Prefer `readonly` fields and read-only contracts (`IReadOnlyList<T>`, `IReadOnlyDictionary<TKey, TValue>`).

## Prefer C# 8 Features
- Use nullable reference types correctly (`string?`, `T?`, null-flow checks).
- Prefer switch expressions for value mapping when clearer than large `switch` statements.
- Use pattern matching (`is`, `is not`, type/property patterns) to simplify guarded casts.
- Prefer using declarations (`using var stream = ...;`) for scoped disposables.
- Use `Index`/`Range` (`^1`, `..`) only when readability is improved.
- Use default interface members only when needed for API compatibility; avoid for simple cases.

## Porting Constraints
- Preserve original semantics from source dumps.
- Keep adaptations minimal and engine-focused (Godot lifecycle, API mapping, integration glue).
- For communication types (`Manager`, `Outgoing`, `Incoming`, `Parser`), follow `docs/COMMUNICATION_EXAMPLES.md` as required structure baseline.

## Modernization Within Fidelity

See docs/FIDELITY_BOUNDARY.md and docs/MODERNIZATION_CATALOG.md. Summary:

- Preserve semantics exactly (control flow, payload order, message IDs, string/numeric values,
  lifecycle timing, error/presence behavior). Modernize representation freely (names, the C# type
  used to express a value, idiomatic constructs) when behavior is byte-for-byte identical.
- Prefer `[Flags] enum` over `static class` of `uint`/`int` flag constants; keep values identical.
- Do not expose `List<object>` or `params object?[]` for source-defined data; use typed overloads.
- String event/parameter names are kept as constants matching source; never a raw literal at a
  call site, never an altered value.
- Use FrozenDictionary/FrozenSet for build-once read-many dispatch tables; Span + BinaryPrimitives
  for the EvaWireFormat byte path. Confirm runtime features per docs/GODOT_RUNTIME_NOTES.md.

## Forbidden Identifiers

No decompiler artifacts in identifiers you write or touch: `class_NNNN`, `IClass_NNNN`,
`var_NNNN`, `param1`/`param2`/... in type, member, parameter, local, or file names. When you touch
a file containing these, de-obfuscate them, keep the `@see` anchor, and log the rename in
docs/RENAME_MAP.md.
