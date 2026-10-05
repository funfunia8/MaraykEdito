# Phase 1 — Multi-Room Gate

## Goal
A single project can contain multiple independent rooms while keeping one Design Model as the source of truth.

## Current behavior
- A project may contain any number of `Room` objects.
- New rooms are placed at a deterministic offset in the current foundation UI to avoid overlap.
- The active room can be switched from the room selector.
- Each active room regenerates its own 2D/3D derived state.
- Room selection clears edit history in this foundation milestone to prevent an undo from crossing room context.
- Save/Open already serializes all rooms and their wall relationships.

## Gate criteria
- Two rooms can coexist in one project.
- Their stable IDs remain distinct after save/reopen.
- Wall relationships remain valid for each room.
- Changing the active room does not mutate the other room.
- The active 2D view is regenerated from the selected room.

## Current environment
The package has not been compiled here because the .NET SDK is unavailable in the current execution environment.

UI stability note: the room selector uses a stable observable collection and does not rebuild its item source during ordinary wall/opening edits.
