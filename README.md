# Design Studio V01 — Technical Foundation 17

This package extends the direct wall endpoint editing loop with direct opening selection and movement.

- Door/window openings are selectable in the 2D plan.
- Dragging an opening moves its offset along the host wall.
- The change is committed through `MoveOpeningCommand`.
- Validation protects the model before the derived state is accepted.
- 2D/3D regeneration remains model-driven.
- Undo/Redo covers opening movement.
- Opening properties show type, offset, and width.
- A small escaped-newline corruption present in the previous WPF editing files has been cleaned up.

Build/test status: not executed here; .NET SDK is unavailable in the current environment.


## Package 15
Opening dimensions are now editable from Properties; validation includes minimum dimensions, wall/height bounds, and overlapping-opening detection. Build remains pending on a Windows machine with the required .NET SDK.


## Package 16
Adds UI-driven door/window creation on the selected wall. Creation is command-based, validated, undoable, and regenerated into the same derived 2D/3D views.


## Package 17
Adds Save/Open project flow using the existing serializer, atomic temp-file replacement, persistent dirty/file-path state, undo-history reset on project load, `.design` as the preferred UI extension, and a project JSON round-trip test that verifies stable IDs and host relationships. The final proprietary container format remains an open ADR decision.

## Package 18 — Autosave & Recovery

This package adds a recovery snapshot store, 30-second dirty-state autosave, startup recovery choice,
and recovery round-trip coverage. The recovery file uses the same project serializer and atomic-write
pattern as normal project saving.


## Package 19 — Multi-Room foundation
- Multiple rooms per project are supported.
- New rooms receive deterministic non-overlapping starter origins in the foundation UI.
- Active room selection is exposed in the WPF shell.
- The active room regenerates independently to 2D/3D state.
- Multi-room persistence is covered by a dedicated test.
- A stale wall test was updated to the current wall API.

Build note: the current execution environment does not contain the .NET SDK, so compilation and runtime tests were not executed here.


## Package 20 — Parametric Cabinet foundation
- Cabinet is now a persistent project object with core manufacturing parameters.
- A parametric service derives carcass parts from the parameters.
- Back thickness drives groove dimensions and derived back-part dimensions.
- Basic edge-banding requirements and part quantities are represented.
- Cabinet parameter changes are undoable through a command.
- Cabinet objects round-trip through the project serializer with stable IDs.
- This remains a rule foundation, not a CNC nesting engine.

Build note: the current execution environment does not contain the .NET SDK, so compilation and runtime tests were not executed here.


## Package 21 additions

- Project-level material objects with stable IDs.
- Cabinet material assignments for carcass, back, door, and edge band.
- Per-part material and edge-band references.
- Renderer-independent cabinet preview state.
- Deterministic cut-list grouping by part dimensions, material, grain, and edge-band flags.
- Material and cabinet-assignment persistence through the existing versioned serializer.
- Legacy cabinets without material assignments remain generatable with warnings.

Build status remains **not built in this environment** because the .NET SDK is unavailable here. Static C#/XML/JSON checks pass.


## Package 22 — Cabinet nesting foundation
- Sheet stock comes from Material sheet dimensions.
- Directional grain constraints are respected deterministically.
- Sheet margins and kerf spacing are explicit options.
- A deterministic row/shelf packer produces repeatable placements.
- Waste/utilization is reported per sheet.
- Candidate remnant rectangles are reported conservatively.
- Parts that cannot fit are returned as explicitly unplaced.
- The nesting service is deliberately replaceable; this is not claimed as an optimal CNC nesting engine.

Build status remains **not built in this environment** because the .NET SDK is unavailable here.

## Package 23 — Cabinet production documentation foundation
- Shop drawing records for front, side and top views with model-derived dimensions.
- Part-level drawing records derived from generated cabinet parts.
- Cut document records with material code, grain, thickness and edge-band requirements.
- Nesting sheet documentation with placements, utilization, waste and candidate remnants.
- Assembly documentation generated from the cabinet construction parameters.
- Stable cabinet document code and explicit revision field.
- Documentation is a derived view; it does not mutate the cabinet model.

Build status remains **not built in this environment** because the .NET SDK is unavailable here.

## Package 24 - Production Output Foundation
- Renderer-neutral production drawing page model.
- Deterministic SVG exporter.
- No PDF library locked into Domain/Application.
- Output derived from ProductionDocumentSet only.

## Package 25 - Hardware & Manufacturing Rules
- HardwareItem catalog object and storage support.
- Cabinet hardware planning derived from explicit policy.
- Hardware quantities flow into assembly and production drawing documents.
- Manufacturing rules produce warnings/errors without mutating the model.

## Package 26 - Cabinet Placement
- Cabinet HostRoomId / Position / Rotation.
- RoomPlan2D cabinet footprints.
- Create/move cabinet through Application layer.
- Storage schema v2 with backward-compatible load of schema 1.

## Package 26 - Cabinet Placement
- Cabinet HostRoomId / Position / Rotation.
- RoomPlan2D cabinet footprints and selection/drag interaction.
- MoveCabinetCommand through Application layer.
- Storage schema v2 with backward-compatible loading of schema 1.
- Boolean visibility converter for Cabinet Properties UI.

## Package 27 - Cabinet Clearance & Collision
- Conservative rotated-cabinet AABB footprint.
- Room-bound validation.
- Opening clearance zones.
- Cabinet-to-cabinet collision and warning clearance.
- Placement validation integrated into move, create, and regeneration flows.


## Package 28 — Foundation V2

This package begins the migration from a cabinet-first technical foundation to a comprehensive interior-design product architecture.

- Generic `ProjectObject.Metadata` now carries category, parent, layer, visibility, locking and 2D placement.
- `Building -> Floor -> Room` hierarchy primitives are introduced without breaking existing Room APIs.
- Generic `ProductDefinition` is independent from Cabinet.
- Generic manufacturing contracts are introduced in `DesignStudio.Manufacturing`.
- Project-wide documentation contracts are introduced in `DesignStudio.Documentation`.
- Independent pricing contracts are introduced in `DesignStudio.Pricing`.
- Existing cabinet manufacturing/documentation code remains as a compatibility subsystem; no destructive rewrite was performed.
- Storage persists Foundation V2 object metadata while remaining compatible with older JSON documents.
- Foundation V2 regression tests were added.

### Validation status
Static XML/project/reference/brace checks completed. The environment does not contain the .NET SDK, so the solution has **not** been compiled or test-executed here. A Windows/.NET build gate is required before accepting this package.
