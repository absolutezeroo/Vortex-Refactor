# AS3 Audit Prompt

Reusable Claude Code prompt for the AS3↔Godot divergence audit. Fill the two paths, run it one
subsystem at a time, and it appends findings to `docs/AS3_DISCREPANCIES.md`.

```
ROLE — AS3<->Godot divergence audit (REPORT ONLY, no code changes)

You audit the Vortex-Refactor Godot/C# port against its AS3 source of truth
to find divergences that SILENTLY DROP load-bearing behavior.

INPUTS
- Port repo:  <FILL: path to Vortex-Refactor>
- AS3 dump:   <FILL: path to WIN63-...-Source>   (every port symbol has an @see -> its AS3 origin)
- Authority:  docs/FIDELITY_BOUNDARY.md — semantics MUST match; representation (names,
              [Flags] enum, typed overloads, Godot glue) MAY differ. Do NOT flag representation.

DIVERGENCE TAXONOMY (hunt these — each is a confirmed real bug class):
  (1) Collapsed/omitted class — AS3 class with no port counterpart.
      Method: inventory AS3 package vs port package, list every missing class.
      Known: habbo/toolbar is 5/38. Confirm and extend to other packages.
  (2) Dropped parameter — port ctor/method missing an arg AS3 has, ESPECIALLY
      IAssetLibrary/`assets` used for bitmap binding
      (e.g. AS3 parse(xml, assets, container) -> port Parse(xml, container)).
  (3) Explicit->global substitution — AS3 resolves via a passed-in IAssetLibrary;
      port reaches for global/static HabboAssetResolver. Flag every skin/icon/render
      path doing this, and VERIFY the global actually resolves the same
      component-scoped asset names (load the relevant *_manifest.xml and check).
  (4) Fail-silent vs fail-loud — port wraps a required lookup in `if (x != null)`
      or try/fallback where AS3 assumes presence and throws. These hide missing assets.
  (5) Stub/TODO replacing real logic — TODO(as3-port)/TODO(assets)/empty body where
      AS3 has behavior. Note what behavior is missing.

METHOD (leaf-first, one subsystem at a time):
  - Resolve each port symbol's @see AS3 path; open BOTH files.
  - Compare: signatures, control flow, asset/event threading, init order, throw-vs-guard.
  - Classify divergence by taxonomy (1-5) + severity: blocks-render / wrong-behavior / cosmetic.
  - If unsure whether a divergence is load-bearing, mark `needs-confirmation`. Do NOT guess a verdict.

OUTPUT — append to docs/AS3_DISCREPANCIES.md:
  - Per package: "Missing classes" inventory (AS3 set minus port set).
  - Table: | Port symbol | @see AS3 (file:line) | Type(1-5) | Severity | Evidence (AS3 line vs port line) | Fix direction |

SCOPE THIS RUN (in order):
  1. habbo/toolbar  (start with BottomBarLeft, ToolbarView, ExtensionView, memenu/*IconLoader)
  2. window skin/asset render path: skin parser (class_3503/SkinParserUtil),
     skin renderers (Bitmap/Text/SkinRenderer), and HabboAssetResolver scope.
  3. core/window components.

CONSTRAINTS:
  - Report only. No edits. Cite exact line numbers from both sides.
  - Source of truth = AS3 dump. Current code wins ONLY for Godot-adaptation glue with no
    AS3 equivalent (SubViewport, Node2D DisplayNode, _Process pump) — expected to differ, don't flag.
  - Respect docs/FIDELITY_BOUNDARY.md: never flag a pure naming/representation difference.
```
