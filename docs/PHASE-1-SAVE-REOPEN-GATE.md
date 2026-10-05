# Phase 1 — Save / Reopen Gate

The desktop shell now has a real project file round-trip:

- Save through `IProjectSerializer`.
- Atomic write uses a temporary file in the target directory before replacement.
- Open restores the exact project ID and object IDs.
- Room wall references and door/window host-wall references survive reload.
- Opening a project clears the old undo/redo history.
- Dirty state becomes clean only after a successful save.
- The prototype uses `.design` as the preferred UI extension, while JSON remains the current development serialization format.

The final proprietary `.design` container is still an ADR decision and is not declared complete by this gate.

Build/test was not executed in this environment because the .NET SDK is unavailable here.
