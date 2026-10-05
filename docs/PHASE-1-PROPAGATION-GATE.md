# Propagation Gate

The model now has an explicit derived-output path:

Design Model
→ Validation
→ 2D Projection
→ 3D Scene Representation

## Important architectural rule

2D and 3D data in this layer are **derived state**, not a second source of truth.

A wall resize mutates the semantic Wall object. The room is then regenerated, producing fresh 2D and 3D representations.

This is deliberately renderer-independent. A future WPF/OpenGL/DirectX/etc. renderer consumes the derived representation rather than owning design truth.

## Verified behavior

- Wall identity remains stable after edits.
- Hosted openings remain attached by stable wall ID.
- 2D wall length reflects the changed semantic wall.
- 3D wall representation reflects the same change.
- Opening projection follows the actual host wall direction.
- Invalid openings block regeneration.
