# Phase 2 — Cabinet Production Documentation Gate

## Goal
Derive production-facing documentation from the same parametric cabinet model used for preview,
cut list and nesting.

## Included
- Three base shop-drawing views: front elevation, side elevation and top plan.
- Explicit overall dimensions and manufacturing notes.
- Part-level drawing records derived from generated cabinet parts.
- Cut document records with material codes, grain and edge-band requirements.
- Nesting sheet records with placements, utilization, waste and candidate remnants.
- Assembly sequence derived from cabinet construction parameters.
- Stable cabinet document code and explicit revision field.

## Architecture rule
Documentation is a derived view. It does not mutate the cabinet model and it does not maintain
an independent source of dimensions or quantities.

## Important boundary
This is a documentation foundation, not yet a final PDF/DWG/DXF publisher. A later rendering/export
layer may consume these records without changing the domain model.

## Build/Test
Compilation and runtime tests are not executed in the current environment because the .NET SDK is unavailable.
Static source structure should be validated on Windows before the next production gate.
