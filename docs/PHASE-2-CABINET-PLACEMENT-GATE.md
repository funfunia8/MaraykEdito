# Phase 2 - Cabinet Placement Gate

## Goal
Promote the parametric cabinet from an isolated manufacturing object to a design object placed inside a Room.

## Model
- Cabinet stores HostRoomId, Position (model-space millimeters), and RotationDegrees.
- Placement is part of the Design Model, not the UI.
- Room 2D projection derives Cabinet footprints from the same Model.

## Interaction
- Create Cabinet creates a parametric cabinet and places it at the active room center.
- Cabinet selection and drag operate through Application commands.
- MoveCabinetCommand preserves Undo/Redo architecture.

## Storage
Project schema is now version 2 on save. Loader accepts schema 1 and 2; placement fields are optional so legacy cabinets remain unplaced and load safely.

## Current limitation
Cabinet collision/clearance against walls and other products is not yet enforced. That is intentionally the next rules/clearance gate.
