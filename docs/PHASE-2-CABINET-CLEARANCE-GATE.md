# Phase 2 - Cabinet Clearance & Collision Gate

## Purpose
Protect the Design Model from invalid cabinet placement while keeping placement rules centralized.

## Current rules
- Cabinet footprint is derived from width, depth and rotation.
- A cabinet must remain inside the room wall bounds.
- Cabinet footprints may not overlap each other.
- Cabinet placement is rejected when its footprint enters an opening clearance zone.
- Very small gaps between cabinets produce warnings.

## Important boundary
The current room representation is rectangular and the footprint check uses a conservative axis-aligned bounding box around the rotated cabinet. This is intentional foundation behavior, not a final polygonal collision kernel.

## Source of truth
The Cabinet Model remains authoritative. The validation service only evaluates it; it does not create alternate geometry.

## Environment
The package was not compiled here because the .NET SDK is unavailable in this execution environment.
