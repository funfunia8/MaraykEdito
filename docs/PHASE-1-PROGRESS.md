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

## Next gate

The next implementation gate is **Advanced Room Geometry Integrity**:

- detect non-adjacent wall intersections
- detect invalid/self-intersecting room boundaries
- define valid wall-junction semantics
- distinguish room boundary walls from internal partitions
- establish robust geometric tolerances and normalization rules
- ensure editing and regeneration remain deterministic for non-rectangular rooms

No visual polish is considered complete until the underlying room geometry remains valid under these cases.
