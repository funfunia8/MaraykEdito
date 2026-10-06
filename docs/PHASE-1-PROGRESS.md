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

## Next gate

The next implementation gate is the **Intelligent Room Core**:

- edit wall geometry
- validate hosted door/window placement
- propagate wall changes to dependent objects
- generate deterministic 2D geometry
- generate a first 3D representation through the geometry abstraction
- undo/redo through commands
- save/reopen without losing relationships

No visual polish is considered complete until this gate passes.
