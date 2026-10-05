# Phase 1 — UI Room Gate

## Goal
Connect the first real UI action to the Design Model and derived representations.

## Implemented
- Create Room button invokes Application-layer workspace operation.
- Default room is created as a real domain Room with four Wall objects.
- Room is regenerated immediately through the validation/projection/scene pipeline.
- 2D plan is exposed as presentation state from the same derived model.
- WPF displays the room plan without owning domain geometry rules.
- English/Arabic labels cover the initial shell.
- UI FlowDirection switches between LTR and RTL from localization state.

## Gate Criteria
- UI does not create domain objects directly: PASS by architecture review.
- 2D view is derived from the same room model: PASS by code path.
- No hard-coded workspace labels in the shell: PASS for current shell labels.
- RTL is state-driven: PASS by converter.
- Build/test execution: pending Windows/.NET SDK environment.
