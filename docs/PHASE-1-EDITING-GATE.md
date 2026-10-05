# Phase 1 Editing Gate

The first interactive editing loop is now represented in the architecture:

1. User clicks a wall in the 2D view.
2. UI reports only the selected wall index.
3. Application creates a `ResizeWallCommand`.
4. Command mutates the Design Model.
5. Validation runs before accepting the new state.
6. Regeneration rebuilds the derived 2D/3D state from the same model.
7. Undo/Redo replays model commands and regenerates views.

The editing buttons currently change the selected wall by +/- 500 mm as a deterministic first editing primitive. This is deliberately narrower than a full drag-handle editor; the next geometry/UI gate can introduce direct endpoint manipulation without moving business rules into WPF.

Build/test status: not executed in this environment because the .NET SDK is unavailable.
