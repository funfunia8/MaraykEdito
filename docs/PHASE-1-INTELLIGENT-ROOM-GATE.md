# Intelligent Room Core — Gate

## Implemented

- Semantic room with four stable wall objects.
- Door and window host relationships.
- Opening validation against host wall length and wall height.
- Wall geometry edit with validation and rollback on invalid state.
- Reversible wall resize command.
- Undo/Redo history.
- Versioned JSON persistence remains intact.

## Architectural meaning

The system now has the beginning of the required propagation chain:

User intent
→ Application operation
→ Domain object mutation
→ Relationship validation
→ deterministic geometry representation

The next step is to make the propagation explicit rather than merely validated:

Wall change
→ dependent opening reposition/validation
→ room boundary regeneration
→ 2D projection
→ 3D representation
→ document dirty state

Only after this chain is stable should the first production UI be connected to it.
