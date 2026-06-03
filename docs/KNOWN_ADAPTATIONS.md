# Known Adaptations

Track intentional Godot/C# adaptations that do not change source behavior.

## Allowed Adaptation Categories
- API replacement: Flash/AS3 API mapped to Godot/.NET equivalent
- Lifecycle translation: AS3 init/update/dispose mapped to Godot lifecycle hooks
- Type/runtime translation: collections, numeric types, nullability, async boundaries

## Entry Template
Use one entry per adaptation:

```md
## [Area] Short Title
- Source reference:
- Target file:
- Original behavior:
- Godot/C# adaptation:
- Why behavior is preserved:
- Risk level (Low/Medium/High):
```

## Rules
- Do not log feature additions here; only technical adaptations.
- If adaptation affects protocol shape or ordering, also update rationale from `docs/SOURCE_CONFLICTS.md` in PR.
- Keep entries concise and evidence-based.
