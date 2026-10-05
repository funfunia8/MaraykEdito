# ADR-002 — Geometry Abstraction

## Status
Accepted for Phase 1.

## Decision
The Design Model will not depend directly on a concrete geometry kernel.

A small `IGeometryEngine` boundary is introduced first. The production geometry kernel will be selected only after a Technical Spike.

## Reason
The application must be able to replace the geometry implementation without rewriting Domain or Application logic.

## Current spike
A deterministic basic geometry adapter is used only to validate contracts, testing, and dependency direction.

## Not approved
This adapter is not the final production geometry engine.
