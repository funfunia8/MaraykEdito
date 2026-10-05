# Phase 1 — Opening Dimensions & Constraints Gate

## Added
- Width/height editing for doors and windows from the Properties panel.
- Sill-height editing for windows.
- Transactional `ResizeOpeningCommand` with Undo/Redo.
- Validation for minimum opening size, wall bounds, vertical bounds, and overlapping openings.
- Regeneration after accepted dimension edits.
- Localized editor text and invalid/rejected edit states.

## Architectural rule
The Properties panel is an input surface only. The authoritative values live in the Design Model, and the application layer validates and regenerates before the UI refreshes.

## Build status
Not built in this environment because the .NET SDK is unavailable.
