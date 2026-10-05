# Phase 2 — Cabinet Material / Edge Band / Cut List Gate

## Goal
Make manufacturing outputs derive from the cabinet parametric model without creating a second source of truth.

## Added
- Project-level Material objects with stable IDs.
- Cabinet material assignments for carcass, back, door, and edge band.
- Per-part material and edge-band references.
- Renderer-independent cabinet preview state.
- Deterministic cut-list grouping by part geometry, material, grain, and edge-band rules.
- Backward-compatible handling for cabinets created before material assignments existed.
- Storage round-trip for materials and cabinet assignments.

## Boundary
Sheet nesting, kerf, waste optimization, remnants, saw/CNC operations, hardware, and final manufacturing documents remain later gates.

## Build status
Not built in this environment because the .NET SDK is unavailable here.
