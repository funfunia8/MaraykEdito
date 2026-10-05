# Phase 1 Progress — Foundation Hardening

## Completed in this iteration

1. Storage is now explicitly versioned (`ProjectDocumentV1`).
2. Persistence uses DTO records rather than serializing domain objects directly.
3. Stable `EntityId` values survive save/load.
4. Domain now distinguishes semantic objects:
   - Room
   - Wall
   - Door
   - Window
5. Doors and windows reference their host wall by stable ID.
6. `IntelligentRoomService` can create a deterministic rectangular room.
7. Basic semantic tests cover:
   - room/wall relationships
   - hosted openings
   - JSON round-trip
8. WPF startup now opens `MainWindow`.
9. C# preview language mode was removed in favor of the latest stable compiler language version.

## Architectural decision

Geometry is still generated from semantic model objects. The domain does not store a concrete rendering/solid-kernel type.

The current JSON serializer is a development baseline only. It is not the final `.design` format.

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
