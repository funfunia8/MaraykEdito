# ADR-003 — Storage Boundary

## Status
Accepted for Phase 1.

## Decision
Project persistence is accessed through `IProjectSerializer`.

JSON is the initial spike format only and is not yet the final `.design` format.

## Requirements for final format
- Versioned schema
- Migration
- Atomic save
- Recovery
- Asset support
- Corruption detection
- Deterministic serialization where practical
- Backward compatibility strategy
