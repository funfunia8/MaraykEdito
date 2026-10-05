# Phase 2 — Cabinet Nesting Foundation Gate

## Objective
Turn the deterministic cabinet cut list into a deterministic sheet-packing foundation without
committing the product to a full optimization/CNC nesting engine yet.

## Included
- Sheet stock dimensions from Material.
- Material grain direction constraints.
- Part-level grain orientation metadata.
- Configurable sheet margin and kerf spacing.
- Deterministic first-fit row packing.
- Per-sheet utilization and waste calculation.
- Conservative candidate remnant rectangles.
- Explicit unplaced parts when a sheet constraint cannot be satisfied.

## Explicitly not claimed
- Optimal nesting.
- CNC-specific lead-in/lead-out.
- Toolpath generation.
- Saw/blade-specific cut sequencing.
- Common-line optimization.
- Final production yield guarantees.

The algorithm is intentionally replaceable behind a service boundary so a stronger optimizer can be
introduced after the geometry and manufacturing data contracts are proven.

## Gate condition
A generated cabinet cut list can be packed reproducibly into sheet stock with grain, kerf, and margin
constraints, and oversized parts are reported as unplaced instead of being silently rotated or clipped.

## Build note
The current execution environment does not contain the .NET SDK, so compilation/runtime tests were not
executed here. Static source/brace/project-file checks are performed before packaging.
