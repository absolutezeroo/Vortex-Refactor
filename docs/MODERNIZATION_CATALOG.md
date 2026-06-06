# Modernization Catalog

Approved modern C# / .NET 9 constructs for the port, each tagged with its **fidelity tier**
(see `docs/FIDELITY_BOUNDARY.md`) and its **Godot runtime safety** (see `docs/GODOT_RUNTIME_NOTES.md`).

Companion to `docs/STYLEGUIDE.md`. The purpose is to make "improve the code" a mechanical rule
instead of a judgment call: when you see the *Use it for* trigger, reach for the listed construct.

> Reality check: .NET 9 does not fix this project's blockers. The blockers are architectural
> (un-ported classes, dropped `IAssetLibrary` threading — see `docs/AS3_DISCREPANCIES.md`).
> The items below are polish, safety, and hot-path performance. Port the missing classes first.

---

## High value — tied to confirmed problems

| Construct | Tier | Godot | Use it for |
|---|---|---|---|
| `ArgumentNullException.ThrowIfNull`, `ObjectDisposedException.ThrowIf`, explicit `throw` | **Tier 1** (restores AS3 fail-loud) | safe | Any lookup the AS3 assumes present. Replaces silent `if (x != null)` guards that the source does not have. This is the fix for the empty-`SkinContainer` bug. |
| `FrozenDictionary<,>` / `FrozenSet<>` | Tier 2 | safe (desktop) | Build-once, read-many dispatch tables: skin-renderer registry, `WidgetType` registry, the `["skin"]/["bitmap"]` renderer map in `SkinParserUtil`, icon-visibility maps. Faster reads on hot paths like `GetSkinRendererByTypeAndStyle`. |
| `Span<byte>` / `ReadOnlySpan<byte>` + `BinaryPrimitives` + `ArrayPool<byte>` | Tier 2 | safe (desktop); re-test under AOT/web | `EvaWireFormat` read/write. `BinaryPrimitives.ReadUInt16BigEndian` makes the big-endian semantics **explicit** (more faithful, not less), and spans/pooling remove per-packet allocations. |

## Medium value — readability and type safety (Tier 2, all safe)

| Construct | Use it for |
|---|---|
| `[Flags] enum` | The `uint`-const "flag" classes (`WindowInvalidation`, `WindowParam`, `WindowState`, `WindowStyle`). Keep the numeric values identical. |
| Typed overloads / a builder | Replacing `List<object>` payloads and `Update(params object?[] args)` style varargs. Emitted bytes / behavior must be identical; lock with a golden test first. |
| Collection expressions `[]`, `[.. spread]` | Replacing `new List<T>()` / array boilerplate. |
| `switch` expressions, list/property patterns | Type dispatch (`is ServiceManager sm`), the renderer-type and widget-type maps. |
| `required` / `init` members | Data structs like `DefaultAttStruct` — make mandatory fields non-optional at construction. |

## Use with caution — can hurt fidelity or aren't what they look like

| Construct | Why caution |
|---|---|
| Primary constructors | Can obscure the `@see` mapping to the AS3 ctor and its `param1/param2` ordering. Use only where it does not hide the source shape. |
| `field` keyword (C# 13) | Same: hides the backing field that may map to a named AS3 member. Marginal benefit. |
| `System.Threading.Lock` (.NET 9) | This is a **threading** primitive. It is **not** the window framework's focus/`lock`-`unlock` reference counting, which is single-threaded UI state. Only use for real cross-thread work (rare; the window system runs on the Godot main thread). |

---

## Adoption rule

1. The construct must be **Tier 2** (representation) — or, like the throw helpers, must *restore*
   Tier 1 semantics. Never use a modern construct to change behavior the source defines.
2. For any **runtime** BCL feature (Frozen*, Span, ArrayPool), confirm it on the export target
   per `docs/GODOT_RUNTIME_NOTES.md` before rolling it out broadly.
3. Hot-path rewrites (`EvaWireFormat`) require a golden test passing before and after.
