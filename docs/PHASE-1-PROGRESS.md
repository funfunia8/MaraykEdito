# Phase 1 Progress — Foundation Hardening

## Completed in this iteration

1. Storage is explicitly versioned through versioned schema generations:
   - `ProjectDocumentV1`
   - `ProjectDocumentV2`
   - `ProjectDocumentV3`
   - `ProjectDocumentV4`
2. Legacy project loading uses explicit migration paths:
   - `V1 -> V2 -> V3 -> V4 -> domain model`
   - `V2 -> V3 -> V4 -> domain model`
   - `V3 -> V4 -> domain model`
   - `V4 -> domain model`
3. Unsupported schema versions are rejected explicitly.
4. Persistence uses DTO records rather than serializing domain objects directly.
5. Stable `EntityId` values survive save/load.
6. Domain now distinguishes semantic objects:
   - Room
   - Wall
   - Door
   - Window
7. Doors and windows reference their host wall by stable ID.
8. `IntelligentRoomService` can create a deterministic rectangular room.
9. Semantic and persistence tests cover:
   - room/wall relationships
   - hosted openings
   - JSON round-trip
   - schema migration
   - rejection of unsupported schema versions
10. WPF startup opens `MainWindow`.
11. C# preview language mode was removed in favor of the latest stable compiler language version.
12. Room boundaries now have explicit directed edge semantics and can reference a measured sub-span of a physical wall.
13. Partial boundary spans propagate consistently through room validation, 2D projection, 3D scene generation, and persistence.
14. Current persistence writes Schema 4 so `WallSpan` data is not lost during save/reopen.

## Architectural decision

Geometry is generated from semantic model objects. The domain does not store a concrete rendering/solid-kernel type.

The current JSON serializer remains a development baseline and is not the final `.design` file format.

Schema migration is explicit and one-way toward the current schema representation:

`ProjectDocumentV1 -> ProjectDocumentV2 -> ProjectDocumentV3 -> ProjectDocumentV4 -> domain model`

`ProjectDocumentV2 -> ProjectDocumentV3 -> ProjectDocumentV4 -> domain model`

`ProjectDocumentV3 -> ProjectDocumentV4 -> domain model`

`ProjectDocumentV4 -> domain model`

Schema versions 1, 2, and 3 are retained as historical read-compatible contracts. Schema 4 is the current write schema.

---

## Intelligent Room Core gate

The Intelligent Room Core baseline is implemented and covered by regression tests.

Completed capabilities include:

- wall geometry editing through commands
- topology-aware wall endpoint movement that preserves connected wall anchors
- room boundary connectivity validation
- room boundary closure validation
- hosted door/window bounds validation
- opening overlap validation
- deterministic 2D room geometry regeneration
- first 3D room representation through the geometry abstraction
- dependent-object propagation after wall changes
- command-based undo/redo
- rollback integrity for invalid workspace edits
- atomic wall-endpoint mutation so a failed edit does not leave partial geometry changes
- explicit JSON schema migration
- save/reopen relationship preservation through the project storage baseline

Historical regression status:

`124/124 tests passing`

The current topology implementation initially validated connectivity and closure only. More advanced geometric validity was subsequently introduced through the dedicated geometry-integrity work below.

---

## Advanced Room Geometry Integrity gate

The initial Advanced Room Geometry Integrity gate is implemented and covered by regression tests.

Completed capabilities include:

- reusable 2D segment relationship classification
- distinction between no relation, endpoint touch, proper intersection, and collinear overlap
- tolerance-aware segment classification
- detection of self-intersecting room boundaries
- detection of non-adjacent boundary contact
- detection of invalid collinear overlap/backtracking
- diagnostic validation errors identifying the conflicting wall IDs
- integration of geometry integrity validation into room validation
- rejection of invalid wall edits through the workspace controller
- full restoration of all affected wall geometry after rejected edits
- preservation of clean undo/redo state after rejected edits
- polygon signed-area calculation
- explicit polygon orientation classification
- rejection of clockwise outer boundaries
- rejection of degenerate and near-zero-area boundaries
- regression coverage for concave and degenerate boundaries

Historical regression status:

`142/142 tests passing`

This gate established reusable geometric primitives but did not yet define a complete spatial-region model.

---

## Room Boundary Semantics gate

The Room Boundary Semantics gate is implemented and materially extended beyond the original Schema 3 baseline.

Completed capabilities include:

- explicit `RoomBoundary` domain concept
- explicit `BoundaryLoop` domain concept
- explicit `OuterLoop` on each room boundary
- preservation of ordered boundary traversal
- `Room.WallIds` retained only as a compatibility projection of `Boundary.OuterLoop.WallIds`
- legacy V1/V2 project data remains loadable through explicit migrations
- existing room consumers continue to operate through the compatibility projection
- signed-area based polygon orientation classification
- outer-loop counter-clockwise orientation validation
- degenerate and near-zero-area boundary rejection
- directed `BoundaryEdge` semantics
- explicit forward or reverse traversal through a physical wall
- support for shared physical walls traversed in opposite directions by adjacent rooms
- `WallSpan` value object for measured sub-ranges of physical walls
- distinction between whole-wall boundaries and partial wall spans
- support for multiple non-overlapping spans of the same physical wall
- rejection of overlapping spans on the same physical wall
- rejection of whole-wall plus partial-span overlap
- deterministic resolution of a `BoundaryEdge` to a directed `WallSegment`
- validation of partial spans against physical wall length
- topology validation against resolved boundary segments rather than complete physical walls
- orientation validation against resolved boundary segments
- partial-span-aware opening membership
- prevention of openings located outside the portion of a wall used by a room boundary
- consistent partial-span propagation to 2D room projection
- consistent partial-span propagation to 3D room scene generation
- Schema 4 persistence for boundary spans
- Schema 3 to Schema 4 migration
- Schema 2 to Schema 3 to Schema 4 migration
- Schema 1 to Schema 2 to Schema 3 to Schema 4 migration
- preservation of `IsReversed` through Schema 4 persistence
- round-trip persistence coverage for whole-wall edges
- round-trip persistence coverage for forward partial spans
- round-trip persistence coverage for reversed partial spans

Current regression status:

`194/194 tests passing`

The current boundary model intentionally supports one explicit outer loop only.

The following are deliberately not considered complete:

- internal partitions as first-class spatial operations
- multiple boundary loops
- holes and voids
- compound regions
- advanced wall-junction semantics
- region containment and point-in-region queries
- robust cabinet placement against arbitrary concave regions
- automatic room splitting
- topological reconstruction after wall edits

---

## Storage Schema 4 gate

Schema 4 is now the current persistence schema because the domain has acquired information that cannot be represented by Schema 3.

Schema 4 preserves:

- wall identity
- boundary traversal direction
- optional partial wall span
- room identity and metadata
- existing project object persistence contracts

A whole-wall boundary is represented by:

- `startOffsetMm = null`
- `endOffsetMm = null`

A partial boundary span is represented explicitly by:

- `startOffsetMm`
- `endOffsetMm`

Direction remains independent from the physical span through `isReversed`.

Schema 3 remains readable and is migrated to Schema 4 with whole-wall semantics preserved.

Current regression status:

`194/194 tests passing`

Schema 4 is considered stable enough for this gate, but the JSON representation is still a development persistence format and is not yet the final production `.design` format.

---

## Current architectural constraints

The following rules are now intentional:

1. A `Wall` is an independent physical entity.
2. A room refers to walls through directed `BoundaryEdge` relationships.
3. A wall must not acquire a single `ParentRoomId`, because a physical wall may be shared by multiple rooms.
4. A `WallSpan` describes a physical interval on a wall; traversal direction belongs to `BoundaryEdge`.
5. `WallSegment` is a derived geometry primitive and is not a topology owner.
6. `Room.WallIds` is a compatibility projection only. New geometry logic must use the explicit boundary edges.
7. The outer boundary is the source of truth for room perimeter geometry.
8. Rendering and scene generation consume derived geometry from the semantic model.
9. Persistence must not silently discard domain information introduced after a schema version.
10. Complex spatial operations must be introduced through explicit topology and geometry semantics rather than by adding ad-hoc object types.

---

## Known technical debt that is intentionally deferred

Several consumers still use `Room.WallIds` because they were written against the earlier compatibility API.

The remaining usages include:

- wall endpoint editing
- workspace/UI wall selection
- intelligent room editing
- project reference validation
- cabinet placement validation
- historical tests and compatibility coverage
- storage compatibility code

This is not treated as evidence that `WallIds` is the architectural source of truth. Those consumers must be migrated deliberately according to their actual semantic responsibility.

In particular:

- wall endpoint editing must become boundary-span/junction aware
- cabinet placement must stop using axis-aligned room bounds as the final spatial test
- UI wall selection must eventually distinguish a physical wall from a boundary edge/span
- opening semantics may eventually require room-side context for shared walls

These changes are deferred until reusable spatial-region and junction semantics exist.

---

## Next gate

The next implementation gate is **Reusable Region Geometry and Containment**.

The purpose of this gate is to establish the spatial foundation required before implementing room splitting and complex spaces.

Target capabilities:

- reusable boundary-to-region construction
- point-in-region containment
- point-on-boundary classification
- region validity independent of UI
- support for concave regions
- robust tolerance-aware containment
- region area and orientation derived from boundary geometry
- containment tests reusable by cabinet placement
- geometry APIs suitable for later room splitting
- deterministic region regeneration from semantic room boundaries
- explicit handling of invalid and degenerate regions

Only after this region foundation is stable should the project introduce:

- internal partition semantics
- wall-junction types
- `SplitRoomByWall`
- multiple boundary loops
- holes/voids
- compound spaces

No visual polish is considered complete until the underlying spatial semantics are explicit, deterministic, and testable.
