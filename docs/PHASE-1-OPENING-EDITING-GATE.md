# Phase 1 — Opening Editing Gate

## Goal
Make door/window openings first-class editable model entities in the 2D designer.

## Current contract
1. Select a door/window from the derived plan.
2. Drag it along its host wall.
3. Convert pointer position to model-space offset.
4. Commit with `MoveOpeningCommand`.
5. Validate the entire room.
6. Regenerate the derived plan/scene from the same model.
7. Undo/Redo the command without changing the opening identity.

## Next
Opening width/height editing will use the same command + validation boundary and will be driven by real model dimensions.
