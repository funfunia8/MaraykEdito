# Phase 1 Progress — Foundation Hardening

## Completed in this iteration

1. Storage is now explicitly versioned with `ProjectDocumentV1` and `ProjectDocumentV2`.
2. Schema 1 projects are explicitly migrated from `ProjectDocumentV1` to the current `ProjectDocumentV2` representation during load.
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
   - schema 1 to schema 2 migration
   - rejection of unsupported schema versions
10. WPF startup now opens `MainWindow`.
11. C# preview language mode was removed in favor of the latest stable compiler language version.

## Architectural decision

Geometry is still generated from semantic model objects. The domain does not store a concrete rendering/solid-kernel type.

The current JSON serializer is a development baseline only. It is not the final `.design` format.

Schema 1 compatibility is implemented as an explicit migration path:

`ProjectDocumentV1 -> ProjectDocumentV2 -> domain model`

Schema 2 documents are loaded directly through `ProjectDocumentV2`.

The current compatibility contract is limited to schema versions 1 and 2. Future schema versions must introduce an explicit migration before they are accepted.


## Intelligent Room Core gate

The Intelligent Room Core baseline is now implemented and covered by regression tests.

Completed capabilities include:

- wall geometry editing through commands
- topology-aware wall endpoint movement that preserves connected wall anchors
- room boundary connectivity validation
- room boundary closure validation
- duplicate wall-reference detection
- hosted door/window bounds validation
- opening overlap validation
- deterministic 2D room geometry regeneration
- first 3D room representation through the geometry abstraction
- dependent-object propagation after wall changes
- command-based undo/redo
- rollback integrity for invalid workspace edits
- atomic wall-endpoint mutation so a failed edit does not leave partial geometry changes
- explicit JSON schema 1 -> schema 2 migration
- save/reopen relationship preservation through the existing project storage baseline

Current regression status:

`124/124 tests passing`

The current topology implementation intentionally validates connectivity and closure only. Advanced geometric validity such as self-intersection, non-manifold wall joins, and more complex boundary semantics is not yet considered complete and must be handled by a later dedicated geometry-integrity gate.


## Advanced Room Geometry Integrity gate

The initial Advanced Room Geometry Integrity gate is now implemented and covered by regression tests.

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

Current regression status:

`142/142 tests passing`

The current implementation still treats the room boundary as a single ordered outer loop, but the boundary model is now explicit rather than implicit.

## Room Boundary Semantics gate

The initial Room Boundary Semantics gate is implemented.

Completed capabilities include:

- explicit `RoomBoundary` domain concept
- explicit `BoundaryLoop` domain concept
- explicit `OuterLoop` on each room boundary
- preservation of wall ordering within the outer boundary loop
- `Room.WallIds` retained as a compatibility projection of `Boundary.OuterLoop.WallIds`
- existing storage format preserved without a schema migration
- existing room consumers continue to operate through the compatibility projection
- domain signed-area calculation
- explicit polygon orientation classification
- outer-loop counter-clockwise orientation validation
- degenerate and near-zero-area boundary rejection
- integration of orientation validation into room validation
- regression coverage for clockwise, counter-clockwise, concave, and degenerate boundaries

Current regression status:

`159/159 tests passing`

The current boundary model intentionally supports one explicit outer loop only. Internal partitions, multiple boundary loops, holes/openings as first-class loops, and advanced wall-junction semantics are not yet complete.

## Next gate

The next implementation gate is **Complex Spaces and Advanced Boundary Semantics**:

- define internal partition semantics
- support multiple boundary loops
- define hole/void semantics as first-class spatial boundaries
- establish valid wall-junction types and connectivity rules
- support non-rectangular and compound spaces without weakening existing boundary integrity rules
- ensure editing, validation, serialization, and regeneration remain deterministic for complex boundaries

No visual polish is considered complete until these underlying spatial semantics are explicit and stable.
