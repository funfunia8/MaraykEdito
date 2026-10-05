# Phase 1 — Dimensions & Constraints Gate

## Added
- Wall length is exposed by the derived 2D model.
- 2D wall dimensions are rendered from model geometry.
- Selected wall is visually distinguished.
- Selected wall length is exposed in the properties panel.
- Minimum wall length constraint is centralized at 300 mm.
- Invalid wall resize is rejected through room validation and remains undoable.

## Architectural rule
Dimensions are derived from the Design Model. The UI does not own authoritative measurements.

## Next
Direct endpoint manipulation, then opening placement/editing with the same command + validation + regeneration pipeline.

## Build status
Not built in this environment; .NET SDK is unavailable here.
