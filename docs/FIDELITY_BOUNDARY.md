# Fidelity Boundary

Resolves the tension between two project rules:

- **Source integrity** (`CONTEXT.md`): behavior must come from the AS3 dumps; do not invent logic.
- **Modern C#** (`STYLEGUIDE.md`): use idiomatic, type-safe constructs.

These do not conflict once we separate **what the source defines** (semantics) from
**how the decompiler happened to express it** (representation). The first is load-bearing
and must be preserved exactly. The second is not source — it is a byproduct of decompilation —
and may be modernized freely.

> The sauce is the control flow, the field/payload order, the event sequencing, and the
> on-the-wire/source string and numeric values. The sauce is **not** `class_3655`, `var_1836`,
> or `param1`.

---

## The Two Tiers

### Tier 1 — Semantic (preserve exactly)

Anything where a change would alter runtime behavior observable from outside the process.
Must mirror the source. Always annotated with `@see`. Any deviation goes through
`docs/SOURCE_CONFLICTS.md` / `docs/KNOWN_ADAPTATIONS.md`.

- Branch conditions, state transitions, control flow, return semantics
- Protocol payload **order** and **types**; message IDs
- Event/parameter **string values** and the **numeric values** of flags and enums
- Lifecycle and event **timing/ordering** (init → unlock → update, invalidate → validate → render)
- Field order **only where** serialization/reflection depends on it (rare; call it out explicitly)

### Tier 2 — Representation (modernize freely)

How a Tier 1 fact is spelled in C#. Changing it produces byte-for-byte identical behavior.
No conflict process required; renames are logged in `docs/RENAME_MAP.md`.

- Identifier names: types, interfaces, fields, properties, parameters, locals, file names
- The **type used to express a value**, provided values/semantics are identical
  (e.g. a `static class` of `uint` consts → `[Flags] enum`; `List<object>` payload → typed overloads)
- `using` layout, access modifiers that do not change behavior, namespace casing
- Idiomatic C# constructs (pattern matching, switch expressions, `using var`) **when behavior is identical**

---

## The Decision Test

> If I changed this, would a captured packet, a state transition, or an event ordering
> differ at runtime?

- **Yes →** Tier 1. Preserve. Keep `@see`. Conflict process if you must deviate.
- **No →** Tier 2. Modernize. Log renames in `RENAME_MAP.md`.

---

## Decompiler Artifacts Are Not Source

`class_NNNN`, `var_NNNN`, `paramN`, and other obfuscated member names are decompilation
byproducts, never authored source. De-obfuscating them is **required** when you touch a file,
not optional. This is already done partially (`class_3596` → `MotionManager`); the rule makes it
consistent and complete.

When renaming a decompiler artifact:

1. Rename the **symbol** and the **file** to the canonical name.
2. Keep the `@see` comment pointing at the original AS3 name — it is the traceability anchor.
3. Add a row to `docs/RENAME_MAP.md` (`AS3 obfuscated name → C# canonical name`).

The `@see` is the link; `RENAME_MAP.md` is the dictionary. Together they preserve full
traceability back to the dumps even after every obfuscated name is gone.

---

## Worked Edge Cases

These are the cases where Tier 1 and Tier 2 sit inside the same construct.

**Flag enums.** The **values** are Tier 1 (`REDRAW = 1`, `RESIZE = 2`, `RELOCATE = 4`, ...). The
**type** is Tier 2. Converting `static class Class3655` of `uint` consts into a `[Flags] enum` is
allowed as long as the numeric values and bitwise semantics are unchanged.

**`List<object>` payloads.** The **order and types written** are Tier 1 (they define the packet).
The **API** expressing them is Tier 2. Replacing `Encode(int, List<object>)` with typed overloads
or a builder is allowed as long as the emitted bytes are identical. Lock this with a golden test
before refactoring.

**String event constants.** The **string value** is Tier 1 — it may travel on the wire or drive
source-matching dispatch (`"rollOver"`, `"INTERNAL_EVENT_UNLOCKED"`). Whether the call site
references it through a typed wrapper instead of a literal is Tier 2. **Never change the literal.**

**Field order in a class.** Usually Tier 2 — C# does not serialize by declaration order.
**Exception:** any type read/written via reflection, manual offset, or order-sensitive
serialization. When in doubt, treat as Tier 1 and note it.

---

## Fail Loud, Not Silent (Tier 1)

Error and presence semantics are **Tier 1**. If the AS3 assumes an asset, interface, or
dependency is present (no null check, direct access) and would throw when it is missing, the
port must throw too. Wrapping a required lookup in `if (x != null) { ... }` or a quiet
try/fallback **changes the semantics**: it converts a loud, diagnosable failure into a silent,
half-initialized state. That is a regression, not a defensive improvement.

This is not hypothetical — it is the root cause of the empty-toolbar bug: the AS3
`initComponent` does `parse(asset.content as XML, ...)` with no guard, so a missing skin
description crashes with "Error initializing window framework". The port wrapped it in
`if (elementDescXml != null)` plus a 3-way fallback, so a failed lookup silently yields an empty
`SkinContainer` and the failure surfaces only as missing graphics much later.

Rule:

- A lookup the AS3 treats as **required** must throw on absence. Use `ArgumentNullException.ThrowIfNull`
  or an explicit `throw`, mirroring where AS3 would have faulted.
- A lookup the AS3 treats as **optional** (it null-checks) may be guarded — match the AS3.
- Never add a guard or fallback that the AS3 does not have. Fail-soft is only allowed where the
  source is fail-soft.

See `docs/MODERNIZATION_CATALOG.md` for the idiomatic throw helpers.

---

## Applying This When Porting

Extends `docs/PORTING_WORKFLOW.md` step 5 (naming) and step 2 (semantics):

1. Port Tier 1 semantics exactly, as today.
2. Express them with Tier 2 modern constructs from the start — do not reproduce decompiler artifacts.
3. If the file already contains artifacts you are touching, de-obfuscate them now (boy-scout rule)
   and log the renames.
4. The `Definition of Done` gate refuses new obfuscated identifiers, so debt cannot re-enter.
