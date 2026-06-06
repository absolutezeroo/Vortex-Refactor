# Godot Runtime Notes

What is safe to use from .NET / C# under Godot's .NET host, and which differences from the AS3
source are **expected** (not bugs). Referenced by `docs/MODERNIZATION_CATALOG.md` and
`docs/FIDELITY_BOUNDARY.md`.

## Target framework

Pin a single TFM and fix the inconsistency between docs: `CONTEXT.md` currently says `net8.0`
while `Directory.Build.props` / `CLAUDE.md` say `net9.0`. Decide, then make all three agree.
.NET 9 requires a Godot version with .NET 9 support — verify against the Godot build actually in
use before relying on it.

## What is safe

- **Compile-time language features** — collection expressions, pattern matching, switch
  expressions, `required`/`init`, target-typed `new`, primary constructors. Always safe; they
  lower to ordinary IL.
- **Runtime BCL on desktop** — `FrozenDictionary`/`FrozenSet`, `Span<T>`/`ReadOnlySpan<T>`,
  `BinaryPrimitives`, `ArrayPool<T>`, throw helpers. Safe under the desktop .NET host.

## What to verify before relying on it

- **AOT / trimming / web (wasm) export** — runtime reflection, dynamic codegen, and some BCL
  paths behave differently. Re-test Frozen*, Span pooling, and any reflection-based code on the
  actual export target if/when you go AOT or web. Desktop is the assumed target until then.
- **Threading** — the window framework runs on the Godot main thread. Do not introduce threads
  casually; `System.Threading.Lock` is for real cross-thread work only, not UI focus locking.

## Expected divergences from AS3 (not discrepancies)

These are Godot-adaptation glue with no Flash equivalent. They legitimately differ from the dump
and must **not** be flagged by the AS3 audit (`docs/AS3_DISCREPANCIES.md`). Keep them logged in
`docs/KNOWN_ADAPTATIONS.md`:

- Flash `stage` / display list → Godot `SubViewport` + `Node2D` `DisplayNode` attached to the tree.
- Flash `Timer` / `getTimer()` / enter-frame → Godot `_Process` / `ProcessFrame`, with the
  `WindowContext.Update` + `Render` pump driven from the frame loop.
- Flash `BitmapData` → Godot `Image` / texture wrappers.
- Stage resize events → `Viewport.SizeChanged`.

The test: if a behavior differs because Flash and Godot express the *same* concept differently,
it is an adaptation. If a behavior differs because the port *dropped* source logic
(a class, a parameter, a throw), it is a discrepancy.
