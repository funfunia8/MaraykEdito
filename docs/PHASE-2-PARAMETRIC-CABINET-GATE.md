# Phase 2 — Parametric Cabinet Gate

## Purpose
Establish the first fabrication-grade parametric product as an architecture proof.

## Model
A Cabinet is a stable project object with:
- width / height / depth
- panel thickness
- back thickness
- construction method
- shelf count
- door count

## Derivation
`CabinetParametricService` derives:
- manufacturing rule values
- groove width / depth / rear offset
- panel parts
- quantity
- basic edge-banding requirements

The derived part list is not a second source of truth. It is regenerated from Cabinet parameters.

## Dependency proof
Changing back thickness changes groove width/depth and the resulting back-part dimensions.
This establishes the intended `parameter -> construction rule -> part geometry` dependency chain.

## Commands
`ResizeCabinetCommand` provides undo/redo for parameter changes.

## Persistence
Cabinet parameters and stable ID are persisted through the existing versioned project serializer.

## Current limitation
This is an architectural/manufacturing rule foundation, not a CNC-ready nesting engine.
Universal joinery standards, material-specific tooling, kerf, grain restrictions, and sheet nesting
remain later manufacturing modules.

## Environment
Not built here because the current environment has no .NET SDK. Windows build/runtime verification remains a release gate.
