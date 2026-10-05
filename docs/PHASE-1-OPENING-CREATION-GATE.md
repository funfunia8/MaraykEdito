# Phase 1 — Opening Creation Gate

## Goal
Create doors and windows from the active room UI while keeping the Design Model as the single source of truth.

## Behavior
- Door/window creation targets the selected wall.
- Default dimensions are applied only as an initial placement aid.
- The created object receives a stable EntityId.
- Creation is committed through CommandHistory and supports Undo/Redo.
- The same room validation service checks bounds, wall height and opening overlaps.
- The selected opening immediately becomes editable through the Properties panel.
- 2D/3D derived state is regenerated after a successful command.

## Architectural rule
The UI requests creation through the Application layer; it never directly mutates Project, Wall, Door or Window objects.

## Current environment
This package has not been compiled here because the .NET SDK is unavailable in the current execution environment.
