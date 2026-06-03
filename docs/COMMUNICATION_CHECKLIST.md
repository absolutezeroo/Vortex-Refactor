# Communication Checklist

Use this checklist for any change in `src/Vortex.Habbo/communication/`.

## Manager
- [ ] Inherits expected base (`Component`) and wires dependencies via `ComponentDependency`.
- [ ] Registers message configuration (`RegisterMessageClasses`).
- [ ] Handles lifecycle correctly (`InitComponent`, listeners, `Dispose` cleanup).
- [ ] Keeps connection/retry logic aligned with source behavior.

## Outgoing (Composer)
- [ ] Implements `IMessageComposer`.
- [ ] Payload order matches source exactly.
- [ ] Field types match protocol expectations.
- [ ] No extra fields or inferred defaults without source proof.

## Incoming (Event)
- [ ] Inherits `MessageEvent` with correct parser type.
- [ ] Exposes parser data via typed accessors.
- [ ] No direct parsing logic inside event class.

## Parser
- [ ] Implements `IMessageParser` with `Flush()` + `Parse()`.
- [ ] `Flush()` resets all parser state deterministically.
- [ ] `Parse()` reads fields sequentially and returns reliable success state.
- [ ] Uses wrapper reads that match source field types (`ReadInteger`, `ReadShort`, etc.).

## Registration & Verification
- [ ] Event/composer IDs mapped in `src/Vortex.Habbo/communication/HabboMessages.cs`.
- [ ] IDs do not collide with existing mappings.
- [ ] Build passes and communication flow smoke-tested.
